using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Options;
using TerryU16.MarathonRunner.Core.Storage;

namespace TerryU16.MarathonRunner.Infrastructures.GoogleCloud;

public class GoogleCloudDownloader : IDownloader
{
    private readonly string _bucketName;

    public GoogleCloudDownloader(IOptions<GoogleCloudOptions> options)
    {
        _bucketName = options.Value.BucketName;
    }

    public async Task DownloadFileAsync(string problemName, string source, string destination)
    {
        var credential = await GoogleCredential.GetApplicationDefaultAsync();
        var storage = await StorageClient.CreateAsync(credential);

        var path = $"{problemName}/{source}";
        await using var stream = new FileStream(destination, FileMode.Create, FileAccess.Write);
        await storage.DownloadObjectAsync(_bucketName, path, stream);
    }
}