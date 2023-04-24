using Microsoft.AspNetCore.Identity;
using Shopping.Data.Entities;
using Shopping.Models;

namespace Shopping.Helpers
{
    public interface IUserHelper
    {
        //Sobrecargamos GetUserAsync
        // Método GetUserAsync pasaremos el email del usuario y nos devuelve el usuario
        Task<User> GetUserAsync(string email);

        //codigo de usuario es un GUID
        Task<User> GetUserAsync(Guid userId);

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

        //Le mandamos usuario password viejo y nuevo
        Task<IdentityResult> ChangePasswordAsync(User user, string oldPassword, string newPassword);

        Task<IdentityResult> UpdateUserAsync(User user);

        //token cadena larga de numeros y letras que tienen que coincidar para enviar el correo
        Task<string> GenerateEmailConfirmationTokenAsync(User user);

        //le mando el token y nos manda un IdentityResult: si fue exitoso fallido error
        Task<IdentityResult> ConfirmEmailAsync(User user, string token);

        //Token de reset
        Task<string> GeneratePasswordResetTokenAsync(User user);

        // Le pasamos usuario token y nueva contraseña
        Task<IdentityResult> ResetPasswordAsync(User user, string token, string password);

    }
}
