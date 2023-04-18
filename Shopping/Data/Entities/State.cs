using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Shopping.Data.Entities
{
    public class State
    {
        public int Id { get; set; }
        //Longitud maxima de caracteres
        [MaxLength(50, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Display(Name = "Estado")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string Name { get; set; }

        //Esto sirve para deserializar el json
        //Relacion 1 a N Un estado pertenece a un pais. UN ESTADO PERTENECE A UN PAIS
        [JsonIgnore]
        public Country Country { get; set; }

        //Un estado tiene una lista (de ciudades)
        public ICollection<City> Cities { get; set; }
        [Display(Name = "Ciudades")]

        public int CitiesNumber => Cities == null ? 0 : Cities.Count;
    }
}
