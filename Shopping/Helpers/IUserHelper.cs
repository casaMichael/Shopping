using Microsoft.AspNetCore.Identity;
using Shopping.Data.Entities;
using Shopping.Models;

namespace Shopping.Helpers
{
    public interface IUserHelper
    {
        // Método GetUserAsync pasaremos el email del usuario y nos devuelve el usuario
        Task<User> GetUserAsync(string email);

        //Método AddUserAsync agregar usuario y pasar el password(mayusucla miniscula caracteres epeciales)
        Task<IdentityResult> AddUserAsync(User user, string password);
        Task<User> AddUserAsync(AddUserViewModel model);

        //Si no exite un rol lo creara
        Task CheckRoleAsync(string roleName);
        
        //Usted usuario pertenece a usuario o admin
        Task AddUserToRoleAsync(User user, string roleName);

        //Verifica es administrador ok sino no
        Task<bool> IsUserInRoleAsync(User user, string roleName);

        //SignInResult es un objeto que nos dice si pudo o no pudo logearse
        Task<SignInResult> LoginAsync(LoginViewModel model);

        Task LogoutAsync();

    }
}
