namespace Shopping.Models
{
    public class HomeViewModel
    {
        //Coleccióno de productos
        public ICollection<ProductsHomeViewModel> Products { get; set; }

        //Cantidad en el carrito de compras
        public float Quantity { get; set; }

    }
}
