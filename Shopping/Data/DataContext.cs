using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Shopping.Data.Entities;

namespace Shopping.Data
{
    public class DataContext : IdentityDbContext<User>
        //IdentityDbContext > fue actualizado el NuGet
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }
        //Por cada entidad hay que mapearlo
        //Colecciones
        public DbSet<Category> Categories { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<State> States{ get; set; }
        public DbSet<Product> Products{ get; set; }
        public DbSet<ProductCategory> ProductCategories{ get; set; }
        public DbSet<ProductImage> ProductImages{ get; set; }
        
        public DbSet<TemporalSale> TemporalSales{ get; set; }
        public DbSet<Sale> Sales{ get; set; }
        public DbSet<SaleDetail> SaleDetails{ get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //Creacion de campo único sobre el campo name para category y country
            modelBuilder.Entity<Category>().HasIndex(c => c.Name).IsUnique();
            modelBuilder.Entity<Country>().HasIndex(c => c.Name).IsUnique();

            //Indice compuesto, al decir CountryId, entity framework de forma interna va a mapear un campo "countryid" que es el forengkey con la clave primaria country y arma la relacion 1 a N
            //(Departamento unico por pais)
            modelBuilder.Entity<State>().HasIndex("Name","CountryId").IsUnique();
            //Validamos un unico departamento y unica ciudad y pais
            modelBuilder.Entity<City>().HasIndex("Name","StateId").IsUnique();
            //Para que no haya dos productos con el mismo nombre
            modelBuilder.Entity<Product>().HasIndex(c => c.Name).IsUnique();
            //Para que el mismo producto no pertenezca a la misma categoria más de una vez
            modelBuilder.Entity<ProductCategory>().HasIndex("ProductId", "CategoryId").IsUnique();
        }
    }
}
