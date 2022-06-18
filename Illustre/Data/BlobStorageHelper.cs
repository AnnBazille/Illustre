using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Data;

public class BlobStorageHelper
{
    private const string ContainerName = "main-container";

    private const string TempDirectory = "temp/";

    private readonly string _blobConnectionString;

    public BlobStorageHelper(IConfiguration configuration)
    {
        _blobConnectionString = configuration
            .GetConnectionString("AzureBlobStorage");
    }

    public string DownloadImage(string filename)
    {
        BlobClient blob = new BlobClient(
                       _blobConnectionString,
                       ContainerName,
                       filename);

        return blob.Uri.AbsoluteUri;
    }

    public async Task UploadImage(string filename, IFormFile file)
    {
        BlobContainerClient blobContainerClient = new BlobContainerClient(
                _blobConnectionString,
                ContainerName);

        await blobContainerClient.CreateIfNotExistsAsync();

        BlobClient blob = blobContainerClient.GetBlobClient(filename);

        var tempFilename = TempDirectory + filename;

        var stream = new FileStream(tempFilename, FileMode.OpenOrCreate);

        await file.CopyToAsync(stream);

        stream.Close();

        await blob.UploadAsync(tempFilename);

        File.Delete(tempFilename);
    }
}
