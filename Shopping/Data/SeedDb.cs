using Shopping.Enums;
using Shopping.Helpers;

namespace Shopping.Data.Entities;

public class SeedDb
{
    private readonly DataContext _context;
    private readonly IUserHelper _userHelper;

    //Inyeccion permanezca en toda la clase (IUserHelper)
    public SeedDb(DataContext context, IUserHelper userHelper)
    {
        _context = context;
        _userHelper = userHelper;
    }
    public async Task SeedAsync()
    {
        //Crea la base de datos y aplica las migraciones
        await _context.Database.EnsureCreatedAsync();
        await CheckCategoriesAsync();
        await CheckCountriesAsync();
        //Chekea los roles
        await CheckRolesAsync();
        //Yopmail correo válido pero falso. Sirve para pruebas.
        await CheckUserAsync("101010", "Michael", "Casa", "casa@yopmail.com", "654 321", "Calle Guadalajara", UserType.Admin);
        await CheckUserAsync("303030", "Alejando", "Ushca", "alex@yopmail.com", "123 456", "Calle Segovia", UserType.User);
    }

    private async Task<User> CheckUserAsync(
    string document,
    string firstName,
    string lastName,
    string email,
    string phone,
    string address,
    UserType userType)
    {
        User user = await _userHelper.GetUserAsync(email);
        if (user == null)
        {
            //Creación de objeto User
            user = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                UserName = email,
                PhoneNumber = phone,
                Address = address,
                Document = document,
                City = _context.Cities.FirstOrDefault(),
                UserType = userType,
            };

            await _userHelper.AddUserAsync(user, "123456");
            await _userHelper.AddUserToRoleAsync(user, userType.ToString());
        }

        return user;
    }

    private async Task CheckRolesAsync()
    {
        await _userHelper.CheckRoleAsync(UserType.Admin.ToString());
        await _userHelper.CheckRoleAsync(UserType.User.ToString());
    }

    private async Task CheckCountriesAsync()
    {
        if (!_context.Countries.Any())
        {
            _context.Countries.Add(new Country
            {
                Name = "España",
                States = new List<State>()
                {
                    new State
                    {
                        Name = "Andalucía",
                        Cities = new List<City>()
                        {
                            new City {Name = "Cádiz"},
                            new City {Name = "Sevilla"},
                            new City {Name = "Málaga"},
                            new City {Name = "Cordoba"},
                        }
                    },
                    new State
                    {
                        Name = "Cataluña",
                        Cities = new List<City>()
                        {
                            new City {Name = "Barcelona"},
                            new City {Name = "Tarragona"},
                            new City {Name = "Gerona"},
                        }
                    }
                }
            });
            _context.Countries.Add(new Country
            {
                Name = "Estados Unidos",
                States = new List<State>()
                {
                    new State
                    {
                        Name = "Florida",
                        Cities = new List<City>()
                        {
                            new City {Name = "Orlando"},
                            new City {Name = "Miami"},
                            new City {Name = "Tampa"},
                            new City {Name = "Fort"},
                        }
                    },
                    new State
                    {
                        Name = "Texas",
                        Cities = new List<City>()
                        {
                            new City {Name = "Houston"},
                            new City {Name = "Dallas"},
                            new City {Name = "El Paso"},
                            new City {Name = "Austin"},
                            new City {Name = "San Antonio"},
                        }
                    }
                }
            });
        }
        await _context.SaveChangesAsync();
    }

    private async Task CheckCategoriesAsync()
    {
        //Any me devuelve si hay un registro por lo menos
        //Si no hay registros agregame las siguientes categorias
        if (!_context.Categories.Any())
        {
            _context.Categories.Add(new Category { Name = "Tecnología" });
            _context.Categories.Add(new Category { Name = "Ropa" });
            _context.Categories.Add(new Category { Name = "Calzado" });
            _context.Categories.Add(new Category { Name = "Belleza" });
            _context.Categories.Add(new Category { Name = "Nutrición" });
            _context.Categories.Add(new Category { Name = "Deportes" });
            _context.Categories.Add(new Category { Name = "Mascotas" });
            _context.Categories.Add(new Category { Name = "Apple" });
            //Guardame los cambios de forma asincrona
            await _context.SaveChangesAsync();
        }
    }

}
