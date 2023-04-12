using Microsoft.EntityFrameworkCore;
using Shopping.Data;
using Shopping.Data.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();



//o de options
builder.Services.AddDbContext<DataContext>(o =>
{
    //Configuración base de datos (inyeccion de dependencias)
    o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

//Preparar el BUILD antes de correrlo
//Cualquier cambio en caliente con esta linea no hay necesidad de ejeuctar y compilar el programa, simplemente refrescar navegador
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();


builder.Services.AddTransient<SeedDb>();
// Importante después de la inyección, sino no corre la app
var app = builder.Build();

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
