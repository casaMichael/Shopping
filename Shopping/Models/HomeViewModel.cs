using Shopping.Common;
using Shopping.Data.Entities;

namespace Shopping.Models
{
    public class HomeViewModel
    {
        //Colección de productos paginados
        public PaginatedList<Product> Products { get; set; }

        //Colección de productos
        //public ICollection<Product> Products { get; set; }

        public IEnumerable<Category> Categories { get; set; }

        //Cantidad en el carrito de compras
        public float Quantity { get; set; }

    }
}
