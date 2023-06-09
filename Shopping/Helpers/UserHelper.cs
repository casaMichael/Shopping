﻿using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shopping.Data;
using Shopping.Data.Entities;
using Shopping.Models;

namespace Shopping.Helpers
{

    public class UserHelper : IUserHelper

    {
        private readonly DataContext _context;
        //userManager administrar usuarios
        private readonly UserManager<User> _userManager;
        //Manejador de roles
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<User> _signInManager;

        //Inyectamos DataContext, UserManager(basado en mi clase de usuarios) y RoleManager
        public UserHelper(DataContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager, SignInManager<User> signInManager)
        {    
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }
    

        public async Task<IdentityResult> AddUserAsync(User user, string password)
        {
            //vaya a la clase usermanager metodo create le pasamos user y password
            return await _userManager.CreateAsync(user, password);
        }

        public async Task<User> AddUserAsync(AddUserViewModel model )
        {
            User user = new User
            {
                Address = model.Address,
                Document = model.Document,
                Email = model.Username,
                FirstName = model.FirstName,
                LastName = model.LastName,
                ImageId = model.ImageId,
                PhoneNumber = model.PhoneNumber,
                City = await _context.Cities.FindAsync(model.CityId),
                UserName = model.Username,
                UserType = model.UserType
            };

            IdentityResult result = await _userManager.CreateAsync(user, model.Password);
            if (result != IdentityResult.Success)
            {
                return null;
            }

            User newUser = await GetUserAsync(model.Username);
            await AddUserToRoleAsync(newUser, user.UserType.ToString());
            return newUser;

        }

        public async Task AddUserToRoleAsync(User user, string roleName)
        {
            //clase usermanager metodo addtorole pasamos usario y nombre del rol
            await _userManager.AddToRoleAsync(user, roleName);
        }

        public async Task<IdentityResult> ChangePasswordAsync(User user, string oldPassword, string newPassword)
        {
            return await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
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

        //IdentityResult: si fue exitosa la confirmación del email
        public async Task<IdentityResult> ConfirmEmailAsync(User user, string token)
        {
            return await _userManager.ConfirmEmailAsync(user, token);
        }

        public async Task<string> GenerateEmailConfirmationTokenAsync(User user)
        {
            return await _userManager.GenerateEmailConfirmationTokenAsync(user);
        }

        public async Task<string> GeneratePasswordResetTokenAsync(User user)
        {
            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }

        public async Task<User> GetUserAsync(string email)
        {
            //Se va al contexto datos y busque usuario
            return await _context.Users
                   //Me devuelve usuario y ciudad
                   .Include(u => u.City)
                   .ThenInclude(c => c.State)
                   .ThenInclude(s => s.Country)
                   .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> GetUserAsync(Guid userId)
        {
            //Se va al contexto datos y busque usuario
            return await _context.Users
                   //Me devuelve usuario y ciudad
                   .Include(u => u.City)
                   .ThenInclude(c => c.State)
                   .ThenInclude(s => s.Country)
                   //El user id sea igual al que me lo pasaron por el parametro
                   .FirstOrDefaultAsync(u => u.Id == userId.ToString());
        }

        public async Task<bool> IsUserInRoleAsync(User user, string roleName)
        {
            //Chekea el rol en usermanager
            return await _userManager.IsInRoleAsync(user, roleName);
        }

        public async Task<SignInResult> LoginAsync(LoginViewModel model)
        {
            //False evita bloquear el usuario después de poner password mal
            return await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, true);
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<IdentityResult> ResetPasswordAsync(User user, string token, string password)
        {
            return await _userManager.ResetPasswordAsync(user, token, password);
        }

        public async Task<IdentityResult> UpdateUserAsync(User user)
        {
            return await _userManager.UpdateAsync(user);
        }
    }
}
