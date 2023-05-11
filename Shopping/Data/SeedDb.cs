using Microsoft.EntityFrameworkCore;
using Shopping.Enums;
using Shopping.Helpers;

namespace Shopping.Data.Entities;

public class SeedDb
{
    private readonly DataContext _context;
    private readonly IUserHelper _userHelper;
    private readonly IBlobHelper _blobHelper;

    //Inyeccion permanezca en toda la clase (IUserHelper)
    public SeedDb(DataContext context, IUserHelper userHelper, IBlobHelper blobHelper)
    {
        _context = context;
        _userHelper = userHelper;
        _blobHelper = blobHelper;
    }
    public async Task SeedAsync()
    {
        //Crea la base de datos y aplica las migraciones
        await _context.Database.EnsureCreatedAsync();
        await CheckCategoriesAsync();
        //Agregar productos
        await CheckProductsAsync();
        await CheckCountriesAsync();
        //Chekea los roles
        await CheckRolesAsync();
        //Yopmail correo válido pero falso. Sirve para pruebas.
        await CheckUserAsync("101010", "Michael", "Casa", "casa@yopmail.com", "654 321", "Calle Guadalajara", "avatar-1.jpg", UserType.Admin);
        await CheckUserAsync("303030", "Alejando", "Ushca", "alex@yopmail.com", "123 456", "Calle Segovia", "avatar-2.jpg", UserType.User);
    }

    private async Task<User> CheckUserAsync(
    string document,
    string firstName,
    string lastName,
    string email,
    string phone,
    string address,
    string image,
    UserType userType)
    {
        User user = await _userHelper.GetUserAsync(email);
        if (user == null)
        {
            Guid imageId = await _blobHelper.UploadBlobAsync($"{Environment.CurrentDirectory}\\wwwroot\\assets\\img\\avatars\\profiles\\{image}", "users");
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
                ImageId = imageId
            };

            await _userHelper.AddUserAsync(user, "123456");
            await _userHelper.AddUserToRoleAsync(user, userType.ToString());

            //El token se lo enviamos al correo, para que cuando el usuario vuelva a confirmar email vuelva con el token.
            //Estas lineas habilitan el usuario
            string token = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
            await _userHelper.ConfirmEmailAsync(user, token);
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
                            new City {Name = "Almería"},
                            new City {Name = "Cádiz"},
                            new City {Name = "Cordoba"},
                            new City {Name = "Granada"},
                            new City {Name = "Huelva"},
                            new City {Name = "Jaén"},
                            new City {Name = "Málaga"},
                            new City {Name = "Sevilla"},
                        }
                    },

                    new State
                    {
                        Name = "Aragón",
                        Cities = new List<City>()
                        {
                            new City {Name = "Huesca"},
                            new City {Name = "Teruel"},
                            new City {Name = "Zaragoza"},
                        }
                    },

                    new State
                    {
                        Name = "Asturias",
                        Cities = new List<City>()
                        {
                            new City {Name = "Asturias"},
                        }
                    },

                    new State
                    {
                        Name = "Baleares Islas",
                        Cities = new List<City>()
                        {
                            new City {Name = "Baleareas Islas"},
                        }
                    },

                    new State
                    {
                        Name = "Canarias",
                        Cities = new List<City>()
                        {
                            new City {Name = "Las Palmas"},
                            new City {Name = "Santa Cruz de Tenerife"},
                        }
                    },

                    new State
                    {
                        Name = "Cantabria",
                        Cities = new List<City>()
                        {
                            new City {Name = "Santander"},
                        }
                    },

                    new State
                    {
                        Name = "Castilla y León",
                        Cities = new List<City>()
                        {
                            new City {Name = "Ávila"},
                            new City {Name = "Burgos"},
                            new City {Name = "León"},
                            new City {Name = "Palencia"},
                            new City {Name = "Salamanca"},
                            new City {Name = "Segovia"},
                            new City {Name = "Soria"},
                            new City {Name = "Valladolid"},
                            new City {Name = "Zamora"},
                        }
                    },

                    new State
                    {
                        Name = "Castilla-La Mancha",
                        Cities = new List<City>()
                        {
                            new City {Name = "Albacete"},
                            new City {Name = "Ciudad Real"},
                            new City {Name = "Cuenca"},
                            new City {Name = "Guadalajara"},
                            new City {Name = "Toledo"},
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
                    },

                    new State
                    {
                        Name = "Comunidad Valenciana",
                        Cities = new List<City>()
                        {
                            new City {Name = "Alicante"},
                            new City {Name = "Castellón"},
                            new City {Name = "Valencia"},
                        }
                    },

                    new State
                    {
                        Name = "Extremadura",
                        Cities = new List<City>()
                        {
                            new City {Name = "Badajoz"},
                            new City {Name = "Cáceres"},
                        }
                    },

                    new State
                    {
                        Name = "Galicia",
                        Cities = new List<City>()
                        {
                            new City {Name = "Coruña"},
                            new City {Name = "Lugo"},
                            new City {Name = "Ourense"},
                            new City {Name = "Pontevedra"},
                        }
                    },

                    new State
                    {
                        Name = "Madrid",
                        Cities = new List<City>()
                        {
                            new City {Name = "Madrid"},
                        }
                    },

                    new State
                    {
                        Name = "Murcia",
                        Cities = new List<City>()
                        {
                            new City {Name = "Murcia"},
                        }
                    },

                    new State
                    {
                        Name = "Navarra",
                        Cities = new List<City>()
                        {
                            new City {Name = "Navarra"},
                        }
                    },

                    new State
                    {
                        Name = "País Vasco",
                        Cities = new List<City>()
                        {
                            new City {Name = "Álava"},
                            new City {Name = "Bizkaia"},
                            new City {Name = "Gipuzkoa"},
                        }
                    },

                    new State
                    {
                        Name = "La Rioja",
                        Cities = new List<City>()
                        {
                            new City {Name = "La Rioja"},
                        }
                    },

                    new State
                    {
                        Name = "Ceuta",
                        Cities = new List<City>()
                        {
                            new City {Name = "Ceuta"},
                        }
                    },

                    new State
                    {
                        Name = "Melilla",
                        Cities = new List<City>()
                        {
                            new City {Name = "Melilla"},
                        }
                    },

                }
            });

            _context.Countries.Add(new Country
            {

                //      COMPLETAR COMUNIDADES Y REGIONES DE PORTUGAL

                Name = "Portugal",
                States = new List<State>()
                {
                    new State
                    {
                        Name = "Norte",
                        Cities = new List<City>()
                        {
                            new City {Name = "Alto Miño"},
                            new City {Name = "Alto Támega"},
                            new City {Name = "Ave"},
                            new City {Name = "Cávado"},
                            new City {Name = "Duero"},
                            new City {Name = "Gran Oporto"},
                            new City {Name = "Támega y Sousa"},
                            new City {Name = "Tierra de Trás-os-Montes"},
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
            _context.Categories.Add(new Category { Name = "Portatiles" });
            _context.Categories.Add(new Category { Name = "PC's" });
            _context.Categories.Add(new Category { Name = "Monitores" });
            _context.Categories.Add(new Category { Name = "Teclados" });
            _context.Categories.Add(new Category { Name = "Ratones" });
            _context.Categories.Add(new Category { Name = "Alfombrillas" });
            _context.Categories.Add(new Category { Name = "Auriculares" });
            _context.Categories.Add(new Category { Name = "Pack Gaming" });
            //Guardame los cambios de forma asincrona
            await _context.SaveChangesAsync();
        }
    }

    private async Task CheckProductsAsync()
    {
        if (!_context.Products.Any())
        {
            await AddProductAsync("Asus Strix G15", 1299M, 30F, new List<string>() { "Portatiles" },
                new List<string>() { "Asus-strix-g15-1.webp", "Asus-strix-g15-2.avif", "Asus-strix-g15-3.webp", "Asus-strix-g15-4.avif", "Asus-strix-g15-5.avif" });
            await AddProductAsync("MSI MAG Infinite S3", 1599M, 15F, new List<string>() { "PC's" },
                new List<string>() { "MSI-MAG-Infinite-S3-13TC-1.avif", "MSI-MAG-Infinite-S3-13TC-2.avif", "MSI-MAG-Infinite-S3-13TC-3.avif", "MSI-MAG-Infinite-S3-13TC-4.avif", "MSI-MAG-Infinite-S3-13TC-5.avif" });
            await AddProductAsync("HP Monitor 25i 165Hz", 189M, 20F, new List<string>() { "Monitores" },
                new List<string>() { "HP-Monitor-25i-165Hz-1.avif", "HP-Monitor-25i-165Hz-2.avif", "HP-Monitor-25i-165Hz-3.avif", "HP-Monitor-25i-165Hz-4.avif", "HP-Monitor-25i-165Hz-5.avif" });
            await AddProductAsync("Razer Barracuda X wireless", 72M, 40F, new List<string>() { "Auriculares" },
                new List<string>() { "Razer-Barracuda-X-wireless-1.webp", "Razer-Barracuda-X-wireless-2.avif", "Razer-Barracuda-X-wireless-3.webp", "Razer-Barracuda-X-wireless-4.avif", "Razer-Barracuda-X-wireless-5.webp" });
            await AddProductAsync("Razer Blackwidow V3 Quartz", 153M, 30F, new List<string>() { "Teclados" },
                new List<string>() { "Razer-Blackwidow-V3-Quartz-1.jpg", "Razer-Blackwidow-V3-Quartz-2.jpg", "Razer-Blackwidow-V3-Quartz-3.jpg", "Razer-Blackwidow-V3-Quartz-4.jpg", "Razer-Blackwidow-V3-Quartz-5.avif" });
            await AddProductAsync("Logitech G903 LightSpeed", 117M, 35F, new List<string>() { "Ratones" },
                new List<string>() { "Logitech-G903-LightSpeed-1.avif", "Logitech-G903-LightSpeed-2.webp", "Logitech-G903-LightSpeed-3.avif", "Logitech-G903-LightSpeed-4.webp" });
            await AddProductAsync("Corsair MM700 RGB", 69.99M, 25F, new List<string>() { "Alfombrillas" },
                new List<string>() { "Corsair-MM700-RGB-1.avif", "Corsair-MM700-RGB-2.avif", "Corsair-MM700-RGB-3.avif" });
            await AddProductAsync("Razer Power Up Bundle V2 ", 139M, 15F, new List<string>() { "Pack Gaming" },
                new List<string>() { "Razer-Power-Up-Bundle-V2-pack.avif", "Razer-Power-Up-Bundle-V2-pack-alfombrilla.avif", "Razer-Power-Up-Bundle-V2-pack-auriculares.webp", "Razer-Power-Up-Bundle-V2-pack-raton.avif", "Razer-Power-Up-Bundle-V2-pack-teclado.avif" });
            await AddProductAsync("Pack Gaming GLab Xenon", 23M, 10F, new List<string>() { "Teclados","Ratones" },
                new List<string>() { "Pack-Gaming-GLab-Xenon.webp", "Pack-Gaming-GLab-Xenon-Raton.webp", "Pack-Gaming-GLab-Xenon-teclado.avif" });

            await _context.SaveChangesAsync();
        }
    }
    private async Task AddProductAsync(string name, decimal price, float stock, List<string> categories, List<string> images)
    {
        Product product = new()
        {
            Description = name,
            Name = name,
            Price = price,
            Stock = stock,
            ProductCategories = new List<ProductCategory>(),
            ProductImages = new List<ProductImage>()
        };

        //Por cada producto adicionamos a la categoria
        foreach (string? category in categories)
        {
            product.ProductCategories.Add(new ProductCategory { Category = await _context.Categories.FirstOrDefaultAsync(c => c.Name == category) });
        }

        //Por cada imagen se agrega al blob de azure
        foreach (string? image in images)
        {
            Guid imageId = await _blobHelper.UploadBlobAsync($"{Environment.CurrentDirectory}\\wwwroot\\assets\\img\\products\\{image}", "products");
            product.ProductImages.Add(new ProductImage { ImageId = imageId });
        }

        _context.Products.Add(product);
    }




}
