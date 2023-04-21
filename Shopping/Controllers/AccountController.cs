using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping.Data;
using Shopping.Data.Entities;
using Shopping.Enums;
using Shopping.Helpers;
using Shopping.Models;

namespace Shopping.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly DataContext _context;
        private readonly ICombosHelper _combosHelper;
        private readonly IBlobHelper _blobHelper;

        public AccountController(IUserHelper userHelper, DataContext context, ICombosHelper combosHelper, IBlobHelper blobHelper)
        {
            _userHelper = userHelper;
            _context = context;
            _combosHelper = combosHelper;
            _blobHelper = blobHelper;
        }

        public IActionResult Login()
        {
            //Si usuario existe va la vista Home acción Index
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new LoginViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                Microsoft.AspNetCore.Identity.SignInResult result = await _userHelper.LoginAsync(model);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Email o contraseña incorrectos.");
            }

            return View(model);
        }

        //Logout no tiene vista(ya que lo direcciona al index del home), si hace falta lo puedo poner
        public async Task<IActionResult> Logout()
        {
            //Borra cookies,sesiones, credenciales...
            await _userHelper.LogoutAsync();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult NotAuthorized()
        {
            return View();
        }

        //GET Crear nuevo usuario
        //Esta acción esta dentro de este controlador porque NO hay que estar logeado para poder registrarse
        public async Task<IActionResult> Register()
        {
            AddUserViewModel model = new()
            {

                Id = Guid.Empty.ToString(),
                //Lista de pais que salen del combo helper
                Countries = await _combosHelper.GetComboCountriesAsync(),
                //Lista de estados, 0 no devuelve nada
                States = await _combosHelper.GetComboStatesAsync(0),
                Cities = await _combosHelper.GetComboCitiesAsync(0),
                UserType = UserType.User,
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(AddUserViewModel model)
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

                LoginViewModel loginViewModel = new LoginViewModel
                {
                    Password = model.Password,
                    //La primera vez tendra que poner el password
                    RememberMe = false,
                    Username = model.Username
                };

                var result2 = await _userHelper.LoginAsync(loginViewModel);

                if (result2.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
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

        public async Task<IActionResult> ChangeUser()
        {
            //Identity nos da el usuario logeado
            User user = await _userHelper.GetUserAsync(User.Identity.Name);

            //Validación por seguridad, usuario no existe
            if (user == null)
            {
                return NotFound();
            }
            //Cogemos el modelo EditUserViewmodel para rellenar campos
            EditUserViewModel model = new()
            {
                Address = user.Address,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                ImageId = user.ImageId,
                //Mostrar ciudad del estado del usuario
                Cities = await _combosHelper.GetComboCitiesAsync(user.City.State.Id),
                //Ciudad seleccionada la que el usuario tiene
                CityId = user.City.Id,
                Countries = await _combosHelper.GetComboCountriesAsync(),
                //País seleccionado
                CountryId = user.City.State.Country.Id,
                StateId = user.City.State.Id,
                States = await _combosHelper.GetComboStatesAsync(user.City.State.Country.Id),
                Id = user.Id,
                Document = user.Document
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeUser(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                //Al entrar el usuario cambiara o no la imagen, asi que la guardamos
                Guid imageId = model.ImageId;

                //Si ImageFile es diferente a nulo, entonces usuario actualizo imagen
                if (model.ImageFile != null)
                {
                    //Subimos la foto al contenedor de users
                    imageId = await _blobHelper.UploadBlobAsync(model.ImageFile, "users");
                }
                //Obtenemos el usuario actual y actualizamos los campos actualizables
                User user = await _userHelper.GetUserAsync(User.Identity.Name);

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Address = model.Address;
                user.PhoneNumber = model.PhoneNumber;
                user.ImageId = imageId;
                user.City = await _context.Cities.FindAsync(model.CityId);
                user.Document = model.Document;

                //Mandamos a la base de datos y despues la accion index
                await _userHelper.UpdateUserAsync(user);
                return RedirectToAction("Index", "Home");
            }

            //Para capturar los datos y que no se pierdan ciudad/estado/pais seleccionados
            model.Countries = await _combosHelper.GetComboCountriesAsync();
            //CountryId es lo que el usuario eligio en el select
            model.States = await _combosHelper.GetComboStatesAsync(model.CountryId);
            model.Cities = await _combosHelper.GetComboCitiesAsync(model.StateId);
            return View(model);
        }
        //Get: le manda la vista, le pide password actual y confirmación
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            //Validamos que todo este correcto
            if (ModelState.IsValid)
            {
                //Validacion si contraseña antigua y actual es la misma
                if (model.OldPassword == model.NewPassword)
                {
                    ModelState.AddModelError(string.Empty, "Ingresar contraseña diferente");
                    return View(model);
                }



                //Buscamos el usuario
                var user = await _userHelper.GetUserAsync(User.Identity.Name);
                if (user != null)
                {
                    var result = await _userHelper.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("ChangeUser");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, result.Errors.FirstOrDefault().Description);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Usuario no encontrado");
                }
            }

            return View(model);
        }


    }
}
