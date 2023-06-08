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

        // (Lista de estados) un pais tiene muchos estados y un estado pertenece a un departamento
        public ICollection<State> States{ get; set; }
        [Display(Name = "Comunidades/Estados")]


        //propiedad de lectura nº de estados de un pais
        //Esto lo calcula el modelo
        //Si states es  igual a null(coleccion nula) da 0 sino devuelveme los states
        public int StatesNumber => States == null ? 0 : States.Count;

        //Cuantos ciudades tiene el estado
        [Display(Name = "Ciudades")]
        public int CitiesNumber => States == null ? 0 : States.Sum(s => s.CitiesNumber);
    }
}
