using System.ComponentModel.DataAnnotations;

namespace Shopping.Data.Entities
{
    public class Country
    {

        public int Id{ get; set; }
        //Longitud maxima de caracteres
        [MaxLength(50, ErrorMessage ="El campo {0} debe tener máximo {1} caractéres.")]
        [Display(Name="Pais")]
        [Required(ErrorMessage ="El campo {0} es obligatorio.")]
        public string Name { get; set; }
        

    }
}
