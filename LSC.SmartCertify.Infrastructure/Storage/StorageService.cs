using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using LSC.SmartCertify.Application.Interfaces.Storage;
using Microsoft.Extensions.Configuration;

namespace LSC.SmartCertify.Infrastructure.Services.Storage
{
    public class StorageService : IStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;
        private readonly IConfiguration configuration;

        public StorageService(IConfiguration configuration)
        {
            string connectionString = configuration["AzureStorage:ConnectionString"];
            _blobServiceClient = new BlobServiceClient(connectionString);
            _containerName = configuration["AzureStorage:UserContainerName"];
            this.configuration = configuration;
        }

        public async Task<string> GenerateSasTokenAsync(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("File name is required.");
            }

            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = blobContainerClient.GetBlobClient(fileName.Split('/').LastOrDefault());

            if (!await blobClient.ExistsAsync())
            {
                throw new Exception("File not found in storage.");
            }

            return GenerateBlobSasToken(blobClient);
        }

        private string GenerateBlobSasToken(BlobClient blobClient)
        {
            if (blobClient.CanGenerateSasUri)
            {
                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = _containerName,
                    BlobName = blobClient.Name,
                    Resource = "b",  // 'b' for blob, 'c' for container
                    ExpiresOn = DateTime.UtcNow.AddSeconds(8000) // Token expires in 1 hour
                };

                sasBuilder.SetPermissions(BlobSasPermissions.Read);

                return blobClient.GenerateSasUri(sasBuilder).Query;
            }

            return "";
        }

        public async Task<string> UploadAsync(byte[] fileData, string fileName, string containerName = "images")
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(string.IsNullOrEmpty(containerName) ? _containerName : containerName);
            await containerClient.CreateIfNotExistsAsync();

            var blobClient = containerClient.GetBlobClient(fileName);

            using (var stream = new MemoryStream(fileData))
            {
                await blobClient.UploadAsync(stream, true);
            }

            return blobClient.Uri.ToString();
        }
    }
}
