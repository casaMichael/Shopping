using Shopping.Common;
using Shopping.Models;

namespace Shopping.Helpers
{
    public interface IOrdersHelper
    {
        //Response es si pudo o no pudo procesar la orden
        Task<Response> ProcessOrderAsync(ShowCartViewModel model);

        Task<Response> CancelOrderAsync(int id);
    }
}
