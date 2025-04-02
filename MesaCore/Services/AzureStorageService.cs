using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace MesaCore.Services
{
    public class AzureStorageService
    {
        private readonly string _connectionString;

        public AzureStorageService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("AzureStorageConnection")!;
        }


        public async Task<string> StoreFiles(string contenedor, IFormFile archivo)
        {
            try
            {
                if (archivo == null || archivo.Length == 0)
                {
                    throw new ApplicationException("El archivo adjunto no puede estar vacío.");
                }

                var extension = Path.GetExtension(archivo.FileName).ToLower();
                var extensionesPermitidas = new[] { ".xlsx", ".xls" };

                if (!extensionesPermitidas.Contains(extension))
                {
                    throw new ApplicationException("Solo se permiten archivos Excel (.xlsx o .xls).");
                }

                var cliente = new BlobContainerClient(_connectionString, contenedor);
                await cliente.CreateIfNotExistsAsync();

                var nombreArchivo = $"{Guid.NewGuid()}{extension}";
                var blob = cliente.GetBlobClient(nombreArchivo);

                var contentType = extension switch
                {
                    ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    ".xls" => "application/vnd.ms-excel",
                    _ => archivo.ContentType
                };

                var blobHttpHeaders = new BlobHttpHeaders
                {
                    ContentType = contentType
                };

                await blob.UploadAsync(archivo.OpenReadStream(), blobHttpHeaders);

                return blob.Uri.ToString();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error al guardar el archivo en azure", ex);
            }
        }
    }
}