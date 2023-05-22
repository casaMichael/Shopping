using System.ComponentModel.DataAnnotations;

namespace Shopping.Data.Entities
{
    public class Category
    {

        public int Id { get; set; }
        //Longitud maxima de caracteres
        [Display(Name = "Categoría")]
        [MaxLength(50, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string Name { get; set; }

        //Una categoria tiene mucho productos
        public ICollection<ProductCategory> ProductCategories{ get; set; }

        //Cuantos productos tengo por cada Categoria
        [Display(Name ="# Productos")]
        public int ProductsNumber => ProductCategories == null ? 0 : ProductCategories.Count();
    }
}
