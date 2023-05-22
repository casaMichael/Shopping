using Microsoft.EntityFrameworkCore;

namespace Shopping.Common
{

    /*
    El método CreateAsync en este código toma el tamaño y el número de la página
    y aplica las instrucciones Skip y Take correspondientes a IQueryable. Cuando se llama a ToListAsync
    en IQueryable, devuelve una lista que solo contiene la página solicitada. Las propiedades HasPreviousPage
    y HasNextPage se pueden usar para habilitar o deshabilitar los botones de página Previous y Next.

    Para crear el objeto PaginatedList<T>, se usa un método CreateAsync en vez de un constructor,
    porque los constructores no pueden ejecutar código asincrónico.
    */

    public class PaginatedList<T> : List<T>
    {
        //Lista de items (productos), total de registros (count), pagina actual, paginazion(de 4 en 4, de 8 en 8 productos)
        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            //Ceiling es para los decimales, redondea (3.4 prodcutos) 
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            AddRange(items);
        }

        //Página actual
        public int PageIndex { get; private set; }

        //Paginación
        public int TotalPages { get; private set; }
        
        //Pagina anterior
        public bool HasPreviousPage => PageIndex > 1;

        //Si no hay paginacion no ire a la siguiente o si estoy en final no habra siguiente página
        public bool HasNextPage => PageIndex < TotalPages;

        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
        {
            int count = await source.CountAsync();
            //Skip salta X registros y Take toma X registros
            List<T> items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }
    }

}
