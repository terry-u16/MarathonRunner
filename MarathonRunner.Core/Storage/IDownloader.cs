namespace TerryU16.MarathonRunner.Core.Storage;

public interface IDownloader
{
    Task DownloadFileAsync(string problemName, string source, string destination, CancellationToken ct = default);
}