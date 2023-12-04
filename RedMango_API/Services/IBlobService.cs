namespace RedMango_API.Services
{
    public interface IBlobService
    {
        Task<string> GetBlob(string blobName,string containerName);   //Blob is that file name in the Azure!
        Task<bool> DeleteBlob(string blobName, string containerName);
        Task<string> UploadBlob(string blobName, string containerName, IFormFile file);  //Creating a new Blob in Azure!
    }
}
