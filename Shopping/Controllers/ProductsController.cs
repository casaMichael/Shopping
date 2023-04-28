using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping.Data;
using Shopping.Data.Entities;
using Shopping.Helpers;
using Shopping.Models;
using System.Data;

namespace Shopping.Controllers
{
    //[Authorize(Roles = "Admin")]
    public class ProductsController : Controller
    {
        private readonly DataContext _context;
        private readonly ICombosHelper _combosHelper;
        private readonly IBlobHelper _blobHelper;

        //Necesitamos context(datos),combos y blob para imagenes en Azure
        public ProductsController(DataContext context, ICombosHelper combosHelper, IBlobHelper blobHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _blobHelper = blobHelper;
        }

        public async Task<IActionResult> Index()
        {
            //Todo esto muestra en el index productos imagenes categorias
            return View(await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
                .ToListAsync());
        }

        //Get
        public async Task<IActionResult> Create()
        {
            CreateProductViewModel model = new()
            {
                //Inyectamos el combosHelper
                Categories = await _combosHelper.GetComboCategoriesAsync(),
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                //Asumimos que no hay imagen , para no perderla
                Guid imageId = Guid.Empty;
                if (model.ImageFile != null)
                {
                    //Subira la imagen al contenedor blob "products"
                    imageId = await _blobHelper.UploadBlobAsync(model.ImageFile, "products");
                }

                Product product = new()
                {
                    Description = model.Description,
                    Name = model.Name,
                    Price = model.Price,
                    Stock = model.Stock,
                };

                product.ProductCategories = new List<ProductCategory>()
                {
                    new ProductCategory
                    {
                        Category = await _context.Categories.FindAsync(model.CategoryId)
                    }
                };

                if (imageId != Guid.Empty)
                {
                    //En la colección de ProductImages creara una nueva lista de productimage
                    product.ProductImages = new List<ProductImage>()
                    {
                        //Imagen sera igual a la que tiene el GUID
                        new ProductImage { ImageId = imageId }
                    };
                }
                try
                {
                    //Product es una composicion en la cual el product internamente tiene Category y productImage y product
                    _context.Add(product);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException dbUpdateException)
                {
                    //Problema duplicado 
                    if (dbUpdateException.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, "Ya existe un producto con este nombre.");
                    }
                    else
                    {
                        //Sino pinta error respectivo
                        ModelState.AddModelError(string.Empty, dbUpdateException.InnerException.Message);
                    }
                }
                catch (Exception exception)
                {
                    ModelState.AddModelError(string.Empty, exception.Message);
                }
            }

            model.Categories = await _combosHelper.GetComboCategoriesAsync();
            return View(model);
        }
        //GET
        //Nos pasan Id que corresponde al id del producto
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            //Buscamos producto
            Product product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            EditProductViewModel model = new()
            {
                Description = product.Description,
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Stock = product.Stock,
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateProductViewModel model)
        {
            //Modelo no valido lo retornamos a notfound
            if (id != model.Id)
            {
                return NotFound();
            }

            try
            {
                Product product = await _context.Products.FindAsync(model.Id);
                //Actualizamos campos
                product.Description = model.Description;
                product.Name = model.Name;
                product.Price = model.Price;
                product.Stock = model.Stock;
                _context.Update(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException dbUpdateException)
            {
                //Validamos que no hayan metido producto con el mismo nombre
                if (dbUpdateException.InnerException.Message.Contains("duplicate"))
                {
                    ModelState.AddModelError(string.Empty, "Ya existe un producto con el mismo nombre.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, dbUpdateException.InnerException.Message);
                }
            }
            catch (Exception exception)
            {
                ModelState.AddModelError(string.Empty, exception.Message);
            }

            return View(model);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //Hacemos los siguientes innerjoin
            Product product = await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
                .FirstOrDefaultAsync(p =>p.Id == id);

            //Si es nulo retorna notfound
            if (product == null)
            {
                return NotFound();
            }
            //Si existe retorna la vista product
            return View(product);
        }

        //GET le manda id que corresponda al producto 
        public async Task<IActionResult> AddImage(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            //Buscamos el producto
            Product product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            //Creamos y mandamos el modelo
            AddProductImageViewModel model = new()
            {
                ProductId = product.Id,
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddImage(AddProductImageViewModel model)
        {
            if (ModelState.IsValid)
            {
                //Mi nuevo codigo de imagen sera igual a cuando lo suba al blob contenedeor products
                Guid imageId = await _blobHelper.UploadBlobAsync(model.ImageFile, "products");
                //Buscamos el producto
                Product product = await _context.Products.FindAsync(model.ProductId);
                //Creamos objeto de la clase ProductImage
                ProductImage productImage = new()
                {
                    Product = product,
                    ImageId = imageId,
                };

                try
                {
                    _context.Add(productImage);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Details), new { Id = product.Id });
                }
                catch (Exception exception)
                {
                    ModelState.AddModelError(string.Empty, exception.Message);
                }
            }
            return View(model);
        }

        public async Task<IActionResult> DeleteImage(int? id)
        {
            //No pedimos confirmación
            if (id == null)
            {
                return NotFound();
            }
            
            ProductImage productImage = await _context.ProductImages
                //Incluimos la imagen del producto, que al eliminarlo nos direccione a la vista details de no ser asi nos volveria al index
                .Include(pi => pi.Product)
                .FirstOrDefaultAsync(pi => pi.Id == id);

            if (productImage == null)
            {
                return NotFound();
            }

            await _blobHelper.DeleteBlobAsync(productImage.ImageId, "products");
            _context.ProductImages.Remove(productImage);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { Id = productImage.Product.Id });
        }

        //GET: me pasan id que corresponde al codigo del producto
        public async Task<IActionResult> AddCategory(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            //Buscamos producto cuyo productoid sea igual al id que me pasan por el parametro
            Product product = await _context.Products
                .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
                .FirstOrDefaultAsync(p => p.Id == id);
            //Si producto no existe pa fuera
            if (product == null)
            {
                return NotFound();
            }
            //Armamos la lista de las categorias que tenga el producto
            List<Category> categories = product.ProductCategories.Select(pc => new Category
            {
                Id = pc.Category.Id,
                Name = pc.Category.Name,

            }).ToList();


            AddCategoryProductViewModel model = new()
            {
                ProductId = product.Id,
                //Me filtra las categorias para que me las excluya del combo
                Categories = await _combosHelper.GetComboCategoriesAsync(categories),
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCategory(AddCategoryProductViewModel model)
        {

            Product product = await _context.Products
                .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
                .FirstOrDefaultAsync(p => p.Id == model.ProductId);

            if (ModelState.IsValid)
            {
                

                ProductCategory productCategory = new()
                {
                    Category = await _context.Categories.FindAsync(model.CategoryId),
                    Product = product,
                };

                try
                {
                    _context.Add(productCategory);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Details), new { Id = product.Id });
                }
                catch (Exception exception)
                {
                    ModelState.AddModelError(string.Empty, exception.Message);
                }
            }

            List<Category> categories = product.ProductCategories.Select(pc => new Category
            {
                Id = pc.Category.Id,
                Name = pc.Category.Name,

            }).ToList();

            //Para que no se me pierdan los datos escritos por el usuario
            model.Categories = await _combosHelper.GetComboCategoriesAsync(categories);
            return View(model);
        }

        public async Task<IActionResult> DeleteCategory(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //Cuando encontremos el productCategory incluimos el producto para poder devolver a la vista details
            ProductCategory productCategory = await _context.ProductCategories
                .Include(pc => pc.Product)
                .FirstOrDefaultAsync(pc => pc.Id == id);
            if (productCategory == null)
            {
                return NotFound();
            }

            _context.ProductCategories.Remove(productCategory);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { Id = productCategory.Product.Id });
        }

        //GET
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //Le mando el product al post
            Product product = await _context.Products
                //Me borra el producto de todas las categorias a las que perten
                .Include(p => p.ProductCategories)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Product model)
        {
            Product product = await _context.Products
                //Me borra el producto de todas las categorias a las que pertenece
                .Include(p => p.ProductImages)
                .Include(p => p.ProductCategories)
                .FirstOrDefaultAsync(p => p.Id == model.Id);

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            //Por cada imagen que tenga el producto lo borrara del blob de azure
            foreach (ProductImage productImage in product.ProductImages)
            {
                await _blobHelper.DeleteBlobAsync(productImage.ImageId, "products");
            }

            return RedirectToAction(nameof(Index));
        }



    }
}
