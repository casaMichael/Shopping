using Microsoft.AspNetCore.Identity;
using Shopping.Enums;
using System.ComponentModel.DataAnnotations;

namespace Shopping.Data.Entities
{
    public class User : IdentityUser
    {
        [Display(Name = "DNI")]
        [MaxLength(20, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string Document { get; set; }

        [Display(Name = "Nombre")]
        [MaxLength(50, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string FirstName { get; set; }

        [Display(Name = "Apellido")]
        [MaxLength(50, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string LastName { get; set; }

        //Grabo la ciudad de forma implicita: quiere decir que no pido pais ni estado
        [Display(Name = "Ciudad")]
        public City City { get; set; }

        [Display(Name = "Dirección")]
        [MaxLength(200, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string Address { get; set; }

        [Display(Name = "Foto")]
        public Guid ImageId { get; set; }
        //Guid: código único alfanumérico como un token

        [Display(Name = "Foto")]
        public string ImageFullPath => ImageId == Guid.Empty
            //Usuario no tiene foto cogera la foto de la maquina local
            ? $"https://localhost:7018/assets/img/no-image.png"
            //Mi blob se llama shoppingcasamichael, aqui cogera la foto de la URL de Azure
            : $"https://shoppingcasamichael.blob.core.windows.net/users/{ImageId}";

        [Display(Name = "Tipo de usuario")]
        public UserType UserType { get; set; }
        
        [Display(Name = "Contacto")]
        public string PhoneNumber { get; set; }


        [Display(Name = "Usuario")]
        public string FullName => $"{FirstName} {LastName}";

        [Display(Name = "Usuario")]
        public string FullNameWithDocument => $"{FirstName} {LastName} - {Document}";

        //Un usuario puede tener mcuhas ventas
        public ICollection<Sale> Sales{ get; set; }


    }
}

