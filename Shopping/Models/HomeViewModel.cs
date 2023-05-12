using Shopping.Data.Entities;

namespace Shopping.Models
{
    public class HomeViewModel
    {
        //Coleccióno de productos
        public ICollection<Product> Products { get; set; }

        //Cantidad en el carrito de compras
        public float Quantity { get; set; }

    }
}
