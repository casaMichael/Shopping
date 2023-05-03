using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shopping.Data;
using Shopping.Data.Entities;
using Shopping.Helpers;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


//o de options
builder.Services.AddDbContext<DataContext>(o =>
{
    //Configuración base de datos (inyeccion de dependencias)
    o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    
    //Conexion BD SQLServer
    //builder.Services.AddControllersWithViews();
});


//TODO: Hacer password más fuerte.
//Mi aplicación va a utilizar Identity, con clase usuario
builder.Services.AddIdentity<User, IdentityRole>(cfg =>
{
    //Creador de tokens por defecto
    cfg.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
    //Mis usuarios tienen que ser confirmados
    cfg.SignIn.RequireConfirmedEmail = true;
    //Usuarios con email único
    cfg.User.RequireUniqueEmail = true;
    //Condiciones de password
    cfg.Password.RequireDigit = false;
    cfg.Password.RequiredUniqueChars = 0;
    cfg.Password.RequireLowercase = false;
    cfg.Password.RequireNonAlphanumeric = false;
    cfg.Password.RequireUppercase = false;
    //Longitud requerida de password
    //cfg.Password.RequiredLength = 6;
    //Bloquear usuario durante 1 minuto
    cfg.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    //Intento 3 veces
    cfg.Lockout.MaxFailedAccessAttempts = 3;
    //Al nuevo usuario tambien lo podemos bloquear
    cfg.Lockout.AllowedForNewUsers = true;

})
    .AddDefaultTokenProviders()
    .AddEntityFrameworkStores<DataContext>();

//Para mostrar página de autorización
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/NotAuthorized";
    options.AccessDeniedPath = "/Account/NotAuthorized";
});


//Inyeccion. Se crear una nueva instancia de la clase cada que es requerido.
builder.Services.AddTransient<SeedDb>();
//Inyecta la interfaz IUserHelper y cada vez que sea llamado le pasamos UserHelper. Esto es para pruebas unitarias.
//Se crea una nueva instancia una sola vez por cada request(petición)

builder.Services.AddScoped<IUserHelper, UserHelper>();
//Cada que alguien llame al combo helper pasame la clase combohelper
builder.Services.AddScoped<ICombosHelper, CombosHelper>();
builder.Services.AddScoped<IBlobHelper, BlobHelper>();
builder.Services.AddScoped<IMailHelper, MailHelper>();

//Preparar el BUILD antes de correrlo
//Cualquier cambio en caliente con esta linea no hay necesidad de ejeuctar y compilar el programa, simplemente refrescar navegador
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

// Importante después de la inyección, sino no corre la app
//var app = builder.Build();

WebApplication? app = builder.Build();

SeedData(app);

//Inyección a mano, es para solicitar solo una vez
void SeedData(WebApplication app)
{
    IServiceScopeFactory? scopedFactory = app.Services.GetService<IServiceScopeFactory>();

    using (IServiceScope? scope = scopedFactory.CreateScope())
    {
        SeedDb? service = scope.ServiceProvider.GetService<SeedDb>();
        service.SeedAsync().Wait();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

//Mostrar paginas de ERROR, esto se va a ejecutar en el Home controller
app.UseStatusCodePagesWithReExecute("/error/{0}");

//Requiere autenticación debido al login/logout
app.UseAuthentication();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
