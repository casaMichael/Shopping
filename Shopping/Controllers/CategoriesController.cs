using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping.Data;
using Shopping.Data.Entities;
using System.Data;

namespace Shopping.Controllers
{
    //Solo administrador puede manipular categorias. Tambien se le puede poner a cada acción

    //TODO: Verificar [Authorize(Roles = "Admin")] al descomentarlo no me muestra nada en la página
    //[Authorize(Roles = "Admin")]
    public class CategoriesController : Controller
    {
        private readonly DataContext _context;

        public CategoriesController(DataContext context) {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            return _context.Categories != null ?
                        //En la vista nos muestra la lista de paises => SELECT * FROM COUNTRIES
                        View(await _context.Categories.ToListAsync()) :
                        Problem("Entity set 'DataContext.Categories'  is null.");
        }
        public IActionResult Create()
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
        }

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

        // GET: Countries/Delete/5
        public async Task<IActionResult> Delete(int? id)
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

    }
}
