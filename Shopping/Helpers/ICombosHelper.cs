using Microsoft.AspNetCore.Mvc.Rendering;
using Shopping.Data.Entities;

namespace Shopping.Helpers
{
    public interface ICombosHelper
    {
        //Listas de categorias
        Task<IEnumerable<SelectListItem>> GetComboCategoriesAsync();

        //Cuando se vaya añadir un producto existente a una categoria, recargo este metodo y en el select no me aparece las categorias a las que pertenece este producto
        Task<IEnumerable<SelectListItem>> GetComboCategoriesAsync(IEnumerable<Category>filter);

        //Lista de países
        Task<IEnumerable<SelectListItem>> GetComboCountriesAsync();

        //Devuelve estados de X país
        Task<IEnumerable<SelectListItem>> GetComboStatesAsync(int countryId);

        //Devuelve lista de ciudades
        Task<IEnumerable<SelectListItem>> GetComboCitiesAsync(int stateId);

    }
}
