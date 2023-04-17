using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping.Data;
using Shopping.Data.Entities;
using Shopping.Models;
using System.Data;

namespace Shopping.Controllers
{
    //Solo el rol administrador puede ejecutar las siguientes acciones
    //[Authorize(Roles = "Admin")]
    public class CountriesController : Controller
    {
        private readonly DataContext _context;

        public CountriesController(DataContext context)
        {
            _context = context;
        }

        // GET: Countries
        //Consulta asincrona, usuario quiere ver paises le pasa el Index
        public async Task<IActionResult> Index()
        {
            /*return _context.Countries != null ?
                        //En la vista nos muestra la lista de paises => SELECT * FROM COUNTRIES
                        View(await _context.Countries.ToListAsync()) :
                        Problem("Entity set 'DataContext.Countries'  is null.");*/
            //Equivalente a hacer un INNER JOIN en la consulta
            return View(await _context.Countries
                .Include(c => c.States)
                .ToListAsync());
        }

        // GET: Countries/Details/5
        //? => puede que sea nulo
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Countries == null)
            {
                return NotFound();
            }

            var country = await _context.Countries
                //Va a incluir el state
                .Include(c => c.States) //Relacion de primer nivel
                                        //Relacion de dos niveles
                .ThenInclude(s => s.Cities)
                .FirstOrDefaultAsync(m => m.Id == id);
            //Si es nulo retorna notfound
            if (country == null)
            {
                return NotFound();
            }
            //Si existe retorna la vista country
            return View(country);
        }

        public async Task<IActionResult> DetailsState(int? id)
        {
            if (id == null || _context.Countries == null)
            {
                return NotFound();
            }

            State state = await _context.States
                //Va a incluir la coleccion de ciudades
                .Include(s => s.Cities)
                .Include(s => s.Country)
                .FirstOrDefaultAsync(m => m.Id == id);
            //Si es nulo retorna notfound
            if (state == null)
            {
                return NotFound();
            }
            //Si existe retorna la vista state
            return View(state);
        }

        public async Task<IActionResult> DetailsCity(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            City city = await _context.Cities
                //Va a incluir la coleccion de ciudades
                .Include(c => c.State)
                .FirstOrDefaultAsync(c => c.Id == id);
            //Si es nulo retorna notfound
            if (city == null)
            {
                return NotFound();
            }
            //Si existe retorna la vista city
            return View(city);
        }
        // GET: Countries/Create
        //Agregado
        [HttpGet]
        public IActionResult Create()
        {
            Country country = new() { States = new List<State>() };
            //Le mandamos a la vista la lista de states
            return View(country);
        }

        // POST: Countries/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Country country)
        {
            //Valido los names pero los states no 
            //le mandamos la lista de states vacia
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(country);
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
                        ModelState.AddModelError(string.Empty, "Ya existe un país con este nombre.");
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
            return View(country);
        }


        //GET
        public async Task<IActionResult> AddState(int? id)
        {
            //Esto es debido a que puede venir el id escrita desde la URL, si no encuentra el id retorna NotFound
            if (id == null || _context.Countries == null)
            {
                return NotFound();
            }
            //EL id corresponde al pais
            Country country = await _context.Countries.FindAsync(id);
            if (country == null)
            {
                return NotFound();
            }

            StateViewModel model = new()
            {
                //lo unico que se es el countryid
                CountryId = country.Id,
            };

            //Le pasamos el model a la vista
            return View(model);
        }

        // POST: Countries/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        //
        public async Task<IActionResult> AddState(StateViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    //Si objeto es valido adiciono estado con lo que venga del modelo
                    State state = new()
                    {
                        Cities = new List<City>(),
                        Country = await _context.Countries.FindAsync(model.CountryId),
                        //Lo que el usuario digito en el name
                        Name = model.Name,
                    };


                    _context.Add(state);
                    await _context.SaveChangesAsync();
                    //Se guarda los cambios pero que devuelva a la vista Details
                    return RedirectToAction(nameof(Details), new { Id = model.CountryId });
                }
                //DbUpdateException fallo actualización
                catch (DbUpdateException dbUpdateException)
                {
                    //Excepcion por duplicado
                    //Excepcion que contiene palabra duplicado
                    if (dbUpdateException.InnerException.Message.Contains("duplicate"))
                    {
                        //
                        ModelState.AddModelError(string.Empty, "Ya existe un departamento/estado en este país.");
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
            return View(model);
        }

        //GET
        public async Task<IActionResult> AddCity(int? id)
        {
            //Esto es debido a que puede venir el id escrita desde la URL, si no encuentra el id retorna NotFound
            if (id == null)
            {
                return NotFound();
            }
            //EL id corresponde al pais
            State state = await _context.States.FindAsync(id);
            if (state == null)
            {
                return NotFound();
            }

            CityViewModel model = new()
            {
                //lo unico que se es el countryid
                StateId = state.Id,
            };

            //Le pasamos el model a la vista
            return View(model);
        }

        // POST: Countries/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        //
        public async Task<IActionResult> AddCity(CityViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    //Creacion de objeto clase CITY
                    City city = new()
                    {
                        State = await _context.States.FindAsync(model.StateId),
                        //Lo que el usuario digito en el name
                        Name = model.Name,
                    };


                    _context.Add(city);
                    await _context.SaveChangesAsync();
                    //Se guarda los cambios pero que devuelva a la vista Details
                    return RedirectToAction(nameof(DetailsState), new { Id = model.StateId });
                }
                //DbUpdateException fallo actualización
                catch (DbUpdateException dbUpdateException)
                {
                    //Excepcion por duplicado
                    //Excepcion que contiene palabra duplicado
                    if (dbUpdateException.InnerException.Message.Contains("duplicate"))
                    {
                        //
                        ModelState.AddModelError(string.Empty, "Ya existe una Ciudad en este departamento/estado.");
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
            return View(model);
        }

        public async Task<IActionResult> EditState(int? id)
        //El id representa un estado
        {
            //Esto es debido a que puede venir el id escrita desde la URL, si no encuentra el id retorna NotFound
            if (id == null || _context.Countries == null)
            {
                return NotFound();
            }

            State state = await _context.States
                .Include(s => s.Country)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (state == null)
            {
                return NotFound();
            }

            StateViewModel model = new()
            {
                CountryId = state.Country.Id,
                //El id sale del state.id y nombre tambien
                Id = state.Id,
                Name = state.Name,
            };
            return View(model);
        }

        // POST: Countries/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditState(int id, StateViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    State state = new()
                    {
                        Id = model.Id,
                        Name = model.Name,
                    };
                    //Nos actualiza el state(estado/departamento)
                    _context.Update(state);
                    await _context.SaveChangesAsync();
                    //Esto al editar y pulsar enter me redirige a la vista Index
                    return RedirectToAction(nameof(Details), new { Id = model.CountryId });
                }
                //Validación de duplicados
                catch (DbUpdateException dbUpdateException)
                {
                    if (dbUpdateException.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, "Ya existe un departamento/estado con el mismo nombre en este pais.");
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
            return View(model);
        }

        //GET
        public async Task<IActionResult> EditCity(int? id)
        //El id representa un estado
        {
            //Esto es debido a que puede venir el id escrita desde la URL, si no encuentra el id retorna NotFound
            if (id == null || _context.Countries == null)
            {
                return NotFound();
            }

            City city = await _context.Cities
                .Include(s => s.State)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (city == null)
            {
                return NotFound();
            }

            CityViewModel model = new()
            {
                StateId = city.State.Id,
                //El id sale del state.id y nombre tambien
                Id = city.Id,
                Name = city.Name,
            };
            return View(model);
        }

        // POST: Countries/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCity(int id, CityViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    City city = new()
                    {
                        Id = model.Id,
                        Name = model.Name,
                    };
                    //Nos actualiza la ciudad
                    _context.Update(city);
                    await _context.SaveChangesAsync();
                    //Esto al editar y pulsar enter me redirige a la vista Index
                    return RedirectToAction(nameof(DetailsState), new { Id = model.StateId });
                }
                //Validación de duplicados
                catch (DbUpdateException dbUpdateException)
                {
                    if (dbUpdateException.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, "Ya existe una ciudad con el mismo nombre en este departamento.");
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
            return View(model);
        }
        public async Task<IActionResult> Edit(int? id)
        {
            //Esto es debido a que puede venir el id escrita desde la URL, si no encuentra el id retorna NotFound
            if (id == null || _context.Countries == null)
            {
                return NotFound();
            }

            var country = await _context.Countries
                .Include(c => c.States)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (country == null)
            {
                return NotFound();
            }
            return View(country);
        }

        // POST: Countries/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Country country)
        {
            if (id != country.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(country);
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
            return View(country);
        }


        // GET: Countries/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Countries == null)
            {
                return NotFound();
            }

            var country = await _context.Countries
                .Include(c => c.States)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (country == null)
            {
                return NotFound();
            }

            return View(country);
        }

        // POST: Countries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Countries == null)
            {
                return Problem("Entity set 'DataContext.Countries'  is null.");
            }
            var country = await _context.Countries.FindAsync(id);
            if (country != null)
            {
                _context.Countries.Remove(country);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DeleteState(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            State state = await _context.States
                //Necesitamos el Country para devolverlo
                .Include(s => s.Country)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (state == null)
            {
                return NotFound();
            }

            return View(state);
        }

        // POST: Countries/Delete/5
        [HttpPost, ActionName("DeleteState")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStateConfirmed(int id)
        {
            if (_context.States == null)
            {
                return Problem("Entity set 'DataContext.Countries'  is null.");
            }
            State state = await _context.States
            .Include(s => s.Country)
            .FirstOrDefaultAsync(s => s.Id == id);
            _context.States.Remove(state);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { Id = state.Country.Id });
        }

        public async Task<IActionResult> DeleteCity(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            City city = await _context.Cities
                //Necesitamos el Country para devolverlo
                .Include(c => c.State)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (city == null)
            {
                return NotFound();
            }

            return View(city);
        }

        // POST: Countries/Delete/5
        [HttpPost, ActionName("DeleteCity")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCityConfirmed(int id)
        {
            if (_context.States == null)
            {
                return Problem("Entity set 'DataContext.Countries'  is null.");
            }
            City city = await _context.Cities
            .Include(c => c.State)
            .FirstOrDefaultAsync(s => s.Id == id);
            _context.Cities.Remove(city);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(DetailsState), new { Id = city.State.Id });
        }

        /*      Esto es inncesario
        private bool CountryExists(int id)
        {
            return (_context.Countries?.Any(e => e.Id == id)).GetValueOrDefault();
        }*/
    }
}
