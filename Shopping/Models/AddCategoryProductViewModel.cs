using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Shopping.Models
{
    public class AddCategoryProductViewModel
    {
        //Codigo de producto para saber a que producto le metemos la categoria
        public int ProductId { get; set; }

        [Display(Name = "Categoría")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione una categoría.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        //Categoria seleccionada
        public int CategoryId { get; set; }

        //Lista de categorias
        public IEnumerable<SelectListItem> Categories { get; set; }

    }
}
