using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Shopping.Helpers
{
    public class BlobHelper : IBlobHelper
    {
        //CloudBlobClient esto instala el NuGet: using Microsoft.WindowsAzure.Storage.Blob;
        private readonly CloudBlobClient _blobClient;

        public BlobHelper(IConfiguration configuration)
        {
            //Se va configuration, busca clave blob y llave ConnectionString (appsettings.json)
            string keys = configuration["Blob:ConnectionString"];
            // CloudStorageAccount.Parse(keys): Busca las Keys, del blob que tengo en memoria
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(keys);
            //Con este metodo puedo subir y bajar una imagen a mi blob
            _blobClient = storageAccount.CreateCloudBlobClient();
        }

        public async Task DeleteBlobAsync(Guid id, string containerName)
        {
            try
            {
                //Referencia el container
                CloudBlobContainer container = _blobClient.GetContainerReference(containerName);
                //Referencia el nombre del blob (users/products)
                CloudBlockBlob blockBlob = container.GetBlockBlobReference($"{id}");
                //Borrar
                await blockBlob.DeleteAsync();
            }
            catch // Si ya borre la imagen e intento borrar otra vez me sacara este metodo.
            {
                throw;
            }
        }

        //IFormFile: captura de imagen por browser
        public async Task<Guid> UploadBlobAsync(IFormFile file, string containerName)
        {
            //Stream: arreglo en memoria de todo el archivo
            Stream stream = file.OpenReadStream();
            return await UploadBlobAsync(stream, containerName);
            
        }

        public async Task<Guid> UploadBlobAsync(byte[] file, string containerName)
        {
            MemoryStream stream = new MemoryStream(file);
            return await UploadBlobAsync(stream, containerName);
        }

        public async Task<Guid> UploadBlobAsync(string image, string containerName)
        {
            //Stream: arreglo en memoria de todo el archivo
            Stream stream = File.OpenRead(image);
            return await UploadBlobAsync(stream, containerName);
        }
        private async Task<Guid> UploadBlobAsync(Stream stream, string containerName)
        {
            //Le damos nombre al archivo, el nombre sera un GUID primero por seguridad
            //y segundo que no lo sobreescriban
            Guid name = Guid.NewGuid();
            //Accedemos al container (users/products)
            CloudBlobContainer container = _blobClient.GetContainerReference(containerName);
            //Creacion de blob con ese nombre 
            CloudBlockBlob blockBlob = container.GetBlockBlobReference($"{name}");
            //Luego sube la foto a ese blob
            await blockBlob.UploadFromStreamAsync(stream);
            return name;
        }
    }
}
