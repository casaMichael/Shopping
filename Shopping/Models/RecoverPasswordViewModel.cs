using System.ComponentModel.DataAnnotations;

namespace Shopping.Models
{
    public class RecoverPasswordViewModel
    {
        //Usuario ingresara el correo
        [Display(Name = "Email")]
        [Required(ErrorMessage = "Correo obligatorio.")]
        [EmailAddress(ErrorMessage = "Debe ingresar un correo válido.")]
        public string Email { get; set; }

    }
}
