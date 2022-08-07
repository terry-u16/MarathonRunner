using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using TerryU16.MarathonRunner.Core.Storage;

namespace MarathonRunner.Infrastructures.AzureBlobStorage;

public class BlobDownloader : IDownloader
{
    private readonly string _connectionString;

    public BlobDownloader(IOptions<BlobClientOption> options)
    {
        var connectionString = options.Value.ConnectionString;
        ArgumentNullException.ThrowIfNull(connectionString);
        _connectionString = connectionString;
    }

    public async Task DownloadFileAsync(string problemName, string source, string destination)
    {
        var containerClient = new BlobContainerClient(_connectionString, problemName);
        var blobClient = containerClient.GetBlobClient(source);
        await blobClient.DownloadToAsync(destination);
    }
}