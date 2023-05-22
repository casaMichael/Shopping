using Microsoft.EntityFrameworkCore;
using Shopping.Common;
using Shopping.Data;
using Shopping.Data.Entities;
using Shopping.Enums;
using Shopping.Models;

namespace Shopping.Helpers
{
    public class OrdersHelper : IOrdersHelper
    {
        private readonly DataContext _context;

        public OrdersHelper(DataContext context)
        {
            _context = context;
        }
        public async Task<Response> ProcessOrderAsync(ShowCartViewModel model)
        {
            //Metodo CheckInventoryAsync para procesar si hay o no stock
            Response response = await CheckInventoryAsync(model);
            if (!response.IsSuccess)
            {
                return response;
            }

            //Creamos una nueva order
            Sale sale = new()
            {
                Date = DateTime.UtcNow,
                User = model.User,
                Remarks = model.Remarks,
                SaleDetails = new List<SaleDetail>(),
                OrderStatus = OrderStatus.Nuevo
            };

            //Por cada registro que haya en el temporalSale lo agregamos al SaleDetail
            foreach (TemporalSale item in model.TemporalSales)
            {
                sale.SaleDetails.Add(new SaleDetail
                {
                    Product = item.Product,
                    Quantity = item.Quantity,
                    Remarks = item.Remarks,
                });

                //Buscamos el producto
                Product product = await _context.Products.FindAsync(item.Product.Id);
                if (product != null)
                {
                    //Por cada producto disminuimos el stock
                    product.Stock -= item.Quantity;
                    _context.Products.Update(product);
                }

                //Limpiamos el carrito de compras, para la siguiente compra
                _context.TemporalSales.Remove(item);
            }

            //Grabamos la venta
            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();
            return response;
        }

        private async Task<Response> CheckInventoryAsync(ShowCartViewModel model)
        {
            Response response = new()
            {
                //  "Asumimos" que hay stock
                IsSuccess = true
                //Afirma que el resultado se ha realizado correctamente.
            };
            
            foreach (TemporalSale item in model.TemporalSales)
            {   
                //Verifico por cada producto en la TemporalSale
                Product product = await _context.Products.FindAsync(item.Product.Id);
                if (product == null)
                {
                    response.IsSuccess = false;
                    response.Message = $"Producto {item.Product.Name}, no disponible";
                    return response;
                }
                if (product.Stock < item.Quantity)
                {
                    response.IsSuccess = false;
                    response.Message = $"Lo sentimos no tenemos existencias suficientes del producto {item.Product.Name}, para tomar su pedido. Por favor disminuir la cantidad o sustituirlo por otro.";
                    return response;
                }
            }
            return response;
        }

        //Cancelar order
        public async Task<Response> CancelOrderAsync(int id)
        {
            //Buscamos order e incluimos detalles producto
            Sale sale = await _context.Sales
                .Include(s => s.SaleDetails)
                .ThenInclude(sd => sd.Product)
                .FirstOrDefaultAsync(s => s.Id == id);

            foreach (SaleDetail saleDetail in sale.SaleDetails)
            {
                //Buscamos producto en cada order 
                Product product = await _context.Products.FindAsync(saleDetail.Product.Id);
                if (product != null)
                {
                    //Cuando lo encontramos lo volvemos a aumentar en el stock
                    product.Stock += saleDetail.Quantity;
                }
            }

            sale.OrderStatus = OrderStatus.Cancelado;
            await _context.SaveChangesAsync();
            return new Response { IsSuccess = true };
        }

    }
}

