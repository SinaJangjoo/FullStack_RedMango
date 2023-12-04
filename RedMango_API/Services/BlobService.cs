using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RedMango_API.Services
{
    public class BlobService : IBlobService
    {
        private readonly BlobServiceClient _blobClient;   // We are all do those upload, delete and get blob inside this service!
        public BlobService(BlobServiceClient blobClient)
        {
            _blobClient = blobClient;
        }
        public async Task<bool> DeleteBlob(string blobName, string containerName)
        {
            BlobContainerClient blobContainerClient = _blobClient.GetBlobContainerClient(containerName); //Get Blob Container in Azure
            BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);  // Get each single Blobs in that Container

            return await blobClient.DeleteIfExistsAsync();
        }

        public async Task<string> GetBlob(string blobName, string containerName)
        {
            BlobContainerClient blobContainerClient = _blobClient.GetBlobContainerClient(containerName); 
            BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);  

            return blobClient.Uri.AbsoluteUri;

        }

        public async Task<string> UploadBlob(string blobName, string containerName, IFormFile file)
        {
            BlobContainerClient blobContainerClient = _blobClient.GetBlobContainerClient(containerName); 
            BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);  
            var httpHeaders = new BlobHttpHeaders
            {
                ContentType = file.ContentType
            };
            var result=await blobClient.UploadAsync(file.OpenReadStream(), httpHeaders);  //upload method
            if (result!=null)
            {
                return await GetBlob(blobName, containerName);
            }
            return "";
        }
    }
}
