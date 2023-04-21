﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping.Data;
using Shopping.Data.Entities;
using Shopping.Enums;
using Shopping.Helpers;
using Shopping.Models;

namespace Shopping.Controllers
{
    //[Authorize(Roles="Admin")]
    public class UsersController : Controller
    {
        //Listar todo los usuarios necesitamos acceder a la base de datos en la que creamos el contructor e inyectamos el dataconetext
        private readonly DataContext _context;
        private readonly IUserHelper _userHelper;
        private readonly IBlobHelper _blobHelper;
        private readonly ICombosHelper _combosHelper;

        //Inyectamos los Helpers
        public UsersController(DataContext context, IUserHelper userHelper, IBlobHelper blobHelper,ICombosHelper combosHelper)
        {
            _context = context;
            _userHelper = userHelper;
            _blobHelper = blobHelper;
            _combosHelper = combosHelper;
        }
        public async Task<IActionResult> Index()
        {
            //Inner Join mostrado en el Index
            return View(await _context.Users
                //Relacion (directa) de primer nivel =>       User(N)---->(1)City
                .Include(u => u.City)
                //Relacion de segundo nivel=>       User(N)---->(1)City(N)---->(1)State
                .ThenInclude(c => c.State)
                //Relacion de tercer nivel=>       User(N)---->(1)City(N)---->(1)State(N)---->(1)Country
                .ThenInclude(s => s.Country)
                .ToListAsync());
        //Esto es una buena practica a la hora de mapearlo y mostrarlo en el index con puntos linea 35 38 41
        }

        //GET Crear nuevo Administrador
        //Esta acción esta dentro de este controlador porque hay que estar logeado para poder registrarse ADMINISTRADOR
        public async Task<IActionResult> Create()
        {
            AddUserViewModel model = new()
            {

                Id = Guid.Empty.ToString(),
                //Lista de pais que salen del combo helper
                Countries = await _combosHelper.GetComboCountriesAsync(),
                //Lista de estados, 0 no devuelve nada
                States = await _combosHelper.GetComboStatesAsync(0),
                Cities = await _combosHelper.GetComboCitiesAsync(0),
                //Lo creamos como administrador
                UserType = UserType.Admin,
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddUserViewModel model)
        {
            //IsValid: valida nombre,apellidos,caracteres especiales contraseñas...
            if (ModelState.IsValid)
            {
                Guid imageId = Guid.Empty;

                if (model.ImageFile != null)
                {
                    //La imagen me lo va a subir al conteneder de users
                    imageId = await _blobHelper.UploadBlobAsync(model.ImageFile, "users");
                }
                model.ImageId = imageId;
                User user = await _userHelper.AddUserAsync(model);
                //Si usuario intenta crear cuenta con mismo correo
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Este correo ya existe.");
                    //Para capturar los datos y que no se pierdan si usuario ya "existe"
                    model.Countries = await _combosHelper.GetComboCountriesAsync();
                    //CountryId es lo que el usuario eligio en el select
                    model.States = await _combosHelper.GetComboStatesAsync(model.CountryId);
                    model.Cities = await _combosHelper.GetComboCitiesAsync(model.StateId);
                    return View(model);
                }

                return RedirectToAction("Index", "Home");
            }

            //Para capturar los datos y que no se pierdan si usuario ya "existe"
            model.Countries = await _combosHelper.GetComboCountriesAsync();
            //CountryId es lo que el usuario eligio en el select
            model.States = await _combosHelper.GetComboStatesAsync(model.CountryId);
            model.Cities = await _combosHelper.GetComboCitiesAsync(model.StateId);
            return View(model);
        }

        //GetStates devuelveme los estados. Views>Account>Register linea 67
        public JsonResult GetStates(int countryId)
        {
            Country country = _context.Countries
                .Include(c => c.States)
                .FirstOrDefault(c => c.Id == countryId);
            if (country == null)
            {
                return null;
            }

            return Json(country.States.OrderBy(d => d.Name));
        }

        public JsonResult GetCities(int stateId)
        {
            State state = _context.States
                .Include(s => s.Cities)
                .FirstOrDefault(s => s.Id == stateId);
            if (state == null)
            {
                return null;
            }

            return Json(state.Cities.OrderBy(c => c.Name));
        }
    }
}
