using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Shopping.Data.Entities
{
    public class City
    {
        public int Id { get; set; }
        //Longitud maxima de caracteres
        [Display(Name = "Ciudad")]
        [MaxLength(50, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string Name { get; set; }

        //Una ciudad pertenece a un estado
        [JsonIgnore]
        public State State { get; set; }

        //Una ciudad tiene muchos usuarios
        public ICollection<User> Users { get; set; }
    }
}
