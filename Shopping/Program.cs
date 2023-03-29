using Microsoft.EntityFrameworkCore;
using Shopping.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();



//o de options
builder.Services.AddDbContext<DataContext>(o =>
{
    //Configuraci�n base de datos (inyeccion de dependencias)
    o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});


//Preparar el BUILD antes de correrlo
//Cualquier cambio en caliente con esta linea no hay necesidad de ejeuctar y compilar el programa, simplemente refrescar navegador
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();


var app = builder.Build();

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
