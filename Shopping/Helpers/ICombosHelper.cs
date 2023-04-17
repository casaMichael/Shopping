using Microsoft.AspNetCore.Mvc.Rendering;

namespace Shopping.Helpers
{
    public interface ICombosHelper
    {
        //Listas de categorias
        Task<IEnumerable<SelectListItem>> GetComboCategoriesAsync();

        //Lista de países
        Task<IEnumerable<SelectListItem>> GetComboCountriesAsync();

        //Devuelve estados de X país
        Task<IEnumerable<SelectListItem>> GetComboStatesAsync(int countryId);

        //Devuelve lista de ciudades
        Task<IEnumerable<SelectListItem>> GetComboCitiesAsync(int stateId);

    }
}
