using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping.Data;
using Shopping.Data.Entities;
using Shopping.Helpers;
using System.Data;
using Vereyon.Web;
using static Shopping.Helpers.ModalHelper;

namespace Shopping.Controllers
{
    //Solo administrador puede manipular categorias. Tambien se le puede poner a cada acción

    //TODO: Verificar [Authorize(Roles = "Admin")] al descomentarlo no me muestra nada en la página
    //[Authorize(Roles = "Admin")]
    public class CategoriesController : Controller
    {
        private readonly DataContext _context;
        private readonly IFlashMessage _flashMessage;

        public CategoriesController(DataContext context, IFlashMessage flashMessage ) {
            _context = context;
            _flashMessage = flashMessage;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _context.Categories
                //Me contara los productos que hay en cada categoria
                .Include(c=> c.ProductCategories)
                .ToListAsync());
        }
        /*public IActionResult Create()
        {
            return View();
        }

        // POST: Countries/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(category);
                    await _context.SaveChangesAsync();
                    //Si ha podido crear el pais lo devolvemos a la accion Index
                    return RedirectToAction(nameof(Index));
                }
                //DbUpdateException fallo actualización
                catch (DbUpdateException dbUpdateException)
                {
                    //Excepcion por duplicado
                    //Excepcion que contiene palabra duplicado
                    if (dbUpdateException.InnerException.Message.Contains("duplicate"))
                    {
                        //
                        ModelState.AddModelError(string.Empty, "Ya existe una categoría con este nombre.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, dbUpdateException.InnerException.Message);
                    }
                }
                //Excepcion general
                catch (Exception exception)
                {
                    ModelState.AddModelError(string.Empty, exception.Message);
                }
            }
            return View(category);
        }*/
        
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
            //Si es nulo retorna notfound
            if (category == null)
            {
                return NotFound();
            }
            //Si existe retorna la vista country
            return View(category);
        }
         /*
        public async Task<IActionResult> Edit(int? id)
        {
            //Esto es debido a que puede venir el id escrita desde la URL, si no encuentra el id retorna NotFound
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                    //Esto al editar y pulsar enter me redirige a la vista Index
                    return RedirectToAction(nameof(Index));
                }
                //Validación de duplicados
                catch (DbUpdateException dbUpdateException)
                {
                    if (dbUpdateException.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, "Ya existe un país con el mismo nombre.");
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
            }
            return View(category);
        }

        */

        // GET: Countries/Delete/5      DELETE GET y POST
        /* public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }
        // POST: Countries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Categories == null)
            {
                return Problem("Entity set 'DataContext.Countries'  is null.");
            }
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        */


        //Bloqueamos por la URL , solo entra por el controlador
        //Podemos BORRAR las vistas _Category, Create, Delete y Edit

        [NoDirectAccess]
        public async Task<IActionResult> Delete(int? id)
        {
            /* Ya no haria falta validar, ya que nunca vendra nulo
            if (id == null)
            {
                return NotFound();
            }*/

            Category category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
            try
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                _flashMessage.Info("Registro eliminado.");
            }
            catch
            {
                _flashMessage.Danger("No se puede borrar la categoría porque tiene registros relacionados.");
            }

            return RedirectToAction(nameof(Index));
        }

        [NoDirectAccess]
        public async Task<IActionResult> AddOrEdit(int id = 0) //Valor por defecto es 0, retornara vista con nueva categoría
        {
            if (id == 0)
            {
                return View(new Category());
            }
            else
            {
                Category category = await _context.Categories.FindAsync(id); //Vendra id de la categoría para editarla 
                if (category == null)
                {
                    return NotFound();
                }

                return View(category);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit(int id, Category category)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (id == 0) //Insert
                    {
                        _context.Add(category);
                        await _context.SaveChangesAsync();
                        _flashMessage.Info("Registro creado.");
                    }
                    else //Update
                    {
                        _context.Update(category);
                        await _context.SaveChangesAsync();
                        _flashMessage.Info("Registro actualizado.");
                    }
                }
                catch (DbUpdateException dbUpdateException)
                {
                    if (dbUpdateException.InnerException.Message.Contains("duplicate"))
                    {
                        _flashMessage.Danger("Ya existe una categoría con el mismo nombre.");
                    }
                    else
                    {
                        _flashMessage.Danger(dbUpdateException.InnerException.Message);
                    }
                    return View(category);
                }
                catch (Exception exception)
                {
                    _flashMessage.Danger(exception.Message);
                    return View(category);
                }

                //Si todo es valido renderiza con la categoria a la vista _ViewAll,
                return Json(new 
                { isValid = true,
                    html = ModalHelper.RenderRazorViewToString(this, "_ViewAll", _context.Categories
                    .Include(c => c.ProductCategories).ToList()) });
            }
            //Si no es valido lo manda a la vista AddOrEdit con lo que haya en el modelo
            return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "AddOrEdit", category) });
        }

    }
}
