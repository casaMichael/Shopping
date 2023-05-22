using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping.Common;
using Shopping.Data;
using Shopping.Data.Entities;
using Shopping.Helpers;
using Shopping.Models;
using System.Diagnostics;

namespace Shopping.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DataContext _context;
        private readonly IUserHelper _userHelper;
        private readonly IOrdersHelper _ordersHelper;

        public HomeController(ILogger<HomeController> logger, DataContext context, IUserHelper userHelper, IOrdersHelper ordersHelper)
        {
            _logger = logger;
            _context = context;
            _userHelper = userHelper;
            _ordersHelper = ordersHelper;
        }

        [HttpGet]
        [HttpPost]
        public async Task<IActionResult> Index(
            string sortOrder,
            string currentFilter,
            string searchProduct,
            int? pageNumber)
        {
            //ordenamos por nombre descendentemente(Sino viene parametro de sortorder, asumimos que viene por nombre)
            //Comprueba si el contenido tiene un valor nulo o vacio
            ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "NameDesc" : "";
            //Ordenamos precio descendente(sortoder viene con valor de price )
            ViewData["PriceSortParm"] = sortOrder == "Price" ? "PriceDesc" : "Price";
            ViewData["CurrentFilter"] = searchProduct;

            if (searchProduct != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchProduct = currentFilter;
            }

            //IQueryable=> consulta en la DB de productos
            IQueryable<Product> query = _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.ProductCategories)
                //Mostrar productos disponibles
                .ThenInclude(pc => pc.Category);

            if (!string.IsNullOrEmpty(searchProduct))
            {
                query = query.Where(p =>
                //ToLower me buscara productos con nombre tanto mayusculas como minisculas
                (p.Name.ToLower().Contains(searchProduct.ToLower()) ||
                //Buscamos productos y/o de cualquier categoria
                 p.ProductCategories.Any(pc => pc.Category.Name.ToLower().Contains(searchProduct.ToLower()))) &&
                 p.Stock > 0);
            }
            else
            {
                query = query.Where(p => p.Stock > 0);
            }


            switch (sortOrder)
            {
                case "NameDesc":
                    query = query.OrderByDescending(p => p.Name);
                    break;
                case "Price":
                    query = query.OrderBy(p => p.Price);
                    break;
                case "PriceDesc":
                    query = query.OrderByDescending(p => p.Price);
                    break;
                default:
                    query = query.OrderBy(p => p.Name);
                    break;
            }

            //Sin ordenación ni búsqueda
            /* List<Product> products = await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.ProductCategories)
                .OrderBy(p => p.Description)
                //Mostrar productos disponibles
                .Where(p => p.Stock > 0)
                .ToListAsync(); */


            int pageSize = 4;

            //Creamos HomeView model que tiene la lista de productos
            HomeViewModel model = new ()
            {
                // ?? validación de nulos
            Products = await PaginatedList<Product>.CreateAsync(query, pageNumber ?? 1, pageSize),
            Categories = await _context.Categories.ToListAsync(),
            };

            //Que nos busque el usuario
            User user = await _userHelper.GetUserAsync(User.Identity.Name);
            if (user != null)
            {
                //La cantidad se sumara de lo que tenga en el temporalsale
                model.Quantity = await _context.TemporalSales
                    .Where(ts => ts.User.Id == user.Id)
                    .SumAsync(ts => ts.Quantity);
            }

            return View(model);
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        //Redireccionar error 404
        [Route("error/404")]
        public IActionResult Error404()
        {
            return View();        
        }

        //Adiciona id del producto
        public async Task<IActionResult> Add (int ? id)
        {
            //Si id viene nulo por la URL retornamos NotFound
            if (id == null)
            {
                return NotFound();
            }

            //Usuario no autenticado
            //Se tiene que identificar el usuario para proceder la compra
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            //Si esta logeado buscamos el producto
            Product product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            //Buscamos el usuario que este logeado con el correo electronico
            User user = await _userHelper.GetUserAsync(User.Identity.Name);
            if (user == null)
            {
                return NotFound();
            }
            //Si usuario esta logeado lo mandamos a la temporalsale
            TemporalSale temporalSale = new()
            {
                Product = product,
                Quantity = 1,
                User = user
            };

            //Agregamos producto al temporalsale de usuario
            _context.TemporalSales.Add(temporalSale);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //GET
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //Buscamos producto
            Product product = await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            //Armamos lista de categorias
            string categories = string.Empty;
            foreach (ProductCategory? category in product.ProductCategories)
            {
                //Concatenamos con ,
                categories += $"{category.Category.Name}, ";
            }
            //A la última le quitamos la ,
            categories = categories.Substring(0, categories.Length - 2);

            //Adicionamos el producto
            AddProductToCartViewModel model = new()
            {
                Categories = categories,
                Description = product.Description,
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                ProductImages = product.ProductImages,
                Quantity = 1,
                Stock = product.Stock,
            };

            //Lo mandamos al formulario
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(AddProductToCartViewModel model)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            //Buscamos producto
            Product product = await _context.Products.FindAsync(model.Id);
            if (product == null)
            {
                return NotFound();
            }

            //Buscamos usuario
            User user = await _userHelper.GetUserAsync(User.Identity.Name);
            if (user == null)
            {
                return NotFound();
            }

            //Agregamos el registro al temporalSale
            TemporalSale temporalSale = new()
            {
                Product = product,
                Quantity = model.Quantity,
                Remarks = model.Remarks,
                User = user
            };

            _context.TemporalSales.Add(temporalSale);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //GET
        [Authorize]
        public async Task<IActionResult> ShowCart()
        {
            User user = await _userHelper.GetUserAsync(User.Identity.Name);
            if (user == null)
            {
                return NotFound();
            }

            //Buscamos las compras agregadas
            List<TemporalSale>? temporalSales = await _context.TemporalSales
                .Include(ts => ts.Product)
                .ThenInclude(p => p.ProductImages)
                //Solo muestra los productos agregados por cada usuario
                .Where(ts => ts.User.Id == user.Id)
                .ToListAsync();

            ShowCartViewModel model = new()
            {
                
                User = user,
                TemporalSales = temporalSales,
            };
            //Lo mandamos a la vista
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ShowCart(ShowCartViewModel model)
        {
            User user = await _userHelper.GetUserAsync(User.Identity.Name);
            if (user == null)
            {
                return NotFound();
            }

            model.User = user;
            model.TemporalSales = await _context.TemporalSales
                .Include(ts => ts.Product)
                .ThenInclude(p => p.ProductImages)
                .Where(ts => ts.User.Id == user.Id)
                .ToListAsync();

            Response response = await _ordersHelper.ProcessOrderAsync(model);
            if (response.IsSuccess)
            {
                //Lo redireccionamos a la accion OrderSuccess
                return RedirectToAction(nameof(OrderSuccess));
            }

            ModelState.AddModelError(string.Empty, response.Message);
            return View(model);
        }


        public async Task<IActionResult> DecreaseQuantity(int? id)
        {
            //Buscamos producto
            if (id == null)
            {
                return NotFound();
            }

            TemporalSale temporalSale = await _context.TemporalSales.FindAsync(id);
            if (temporalSale == null)
            {
                return NotFound();
            }

            if (temporalSale.Quantity > 1)
            {
                temporalSale.Quantity--;
                _context.TemporalSales.Update(temporalSale);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(ShowCart));
        }

        public async Task<IActionResult> IncreaseQuantity(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            TemporalSale temporalSale = await _context.TemporalSales.FindAsync(id);
            if (temporalSale == null)
            {
                return NotFound();
            }

            temporalSale.Quantity++;
            _context.TemporalSales.Update(temporalSale);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ShowCart));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //Busca el temporalSale y lo elimina de la lista
            TemporalSale temporalSale = await _context.TemporalSales.FindAsync(id);
            if (temporalSale == null)
            {
                return NotFound();
            }

            _context.TemporalSales.Remove(temporalSale);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ShowCart));
        }


        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            TemporalSale temporalSale = await _context.TemporalSales.FindAsync(id);
            if (temporalSale == null)
            {
                return NotFound();
            }

            //Lo manda al post
            EditTemporalSaleViewModel model = new()
            {
                Id = temporalSale.Id,
                Quantity = temporalSale.Quantity,
                Remarks = temporalSale.Remarks,
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditTemporalSaleViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    //Actualizamos y guardamos en base de datos
                    TemporalSale temporalSale = await _context.TemporalSales.FindAsync(id);
                    temporalSale.Quantity = model.Quantity;
                    temporalSale.Remarks = model.Remarks;
                    _context.Update(temporalSale);
                    await _context.SaveChangesAsync();
                }
                catch (Exception exception)
                {
                    ModelState.AddModelError(string.Empty, exception.Message);
                    return View(model);
                }

                //Si toda va bien lo mandamos al metodo ShowCart
                return RedirectToAction(nameof(ShowCart));
            }

            return View(model);
        }

        [Authorize]
        public IActionResult OrderSuccess()
        {
            return View();
        }

    }
}