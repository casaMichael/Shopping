using Microsoft.AspNetCore.Identity;
using Shopping.Data;
using Shopping.Data.Entities;

namespace Shopping.Helpers
{
    public class UserHelper : IUserHelper

    {
        public DataContext _context { get; }
        public UserManager<User> _userManager { get; }
        public RoleManager<IdentityRole> _roleManager { get; }

        //Inyectamos DataContext, UserManager(basado en mi clase de usuarios) y RoleManager
        public UserHelper(DataContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager) 
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IdentityResult> AddUserAsync(User user, string password)
        {
            //vaya a la clase usermanager metodo create le pasamos user y password
            return await _userManager.CreateAsync(user, password);
        }

        public async Task AddUserToRoleAsync(User user, string roleName)
        {
            //claser usermanager metodo addtorole pasamos usario y nombre del rol
            await _userManager.AddToRoleAsync(user, roleName);
        }

        public async Task CheckRoleAsync(string roleName)
        {
            //Metodo 
            bool roleExists = await _roleManager.RoleExistsAsync(roleName);
            //Si rol no existe lo crea
            if (!roleExists)
            {
                //se dirige al metodo crear rol
                await _roleManager.CreateAsync(new IdentityRole
                {
                    //Crear rol con el nombre que el usuario mando
                    Name = roleName
                });
            }

        }

        public Task<User> GetUserAsync(string email)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsUserInRoleAsync(User user, string roleName)
        {
            throw new NotImplementedException();
        }
    }
}
