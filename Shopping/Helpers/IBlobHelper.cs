namespace Shopping.Helpers
{
    public interface IBlobHelper
    {
        //IFornFile es seleccionar un archivo
        Task<Guid> UploadBlobAsync(IFormFile file, string containerName);

        //Nombre de contenedor 
        Task<Guid> UploadBlobAsync(byte[] file, string containerName);

        //El image corresponde a la ruta fisica de la maquina local, lo sube al contenedor
        Task<Guid> UploadBlobAsync(string image, string containerName);

        //Borrar definitivo
        Task DeleteBlobAsync(Guid id, string containerName);
    }
}
