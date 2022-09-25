using Microsoft.Extensions.Logging;
using TerryU16.MarathonRunner.Core.Executors;
using TerryU16.MarathonRunner.Core.Storage;

namespace TerryU16.MarathonRunner.Infrastructures.GoogleCloud;

public class GoogleCloudExecutor : Executor
{
    private readonly IDownloader _downloader;

    public GoogleCloudExecutor(ILogger<GoogleCloudExecutor> logger, IDownloader downloader) : base(logger)
    {
        _downloader = downloader;
    }

    protected override async Task PrepareFilesAsync(string problemName, IEnumerable<string> filePaths, CancellationToken ct = default)
    {
        foreach (var filePath in filePaths)
        {
            var parent = Directory.GetParent(filePath);
            if (!parent?.Exists ?? true)
            {
                parent?.Create();
            }
            await _downloader.DownloadFileAsync(problemName, filePath, filePath, ct);
        }
    }
}