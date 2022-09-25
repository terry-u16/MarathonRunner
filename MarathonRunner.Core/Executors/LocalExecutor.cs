using Microsoft.Extensions.Logging;

namespace TerryU16.MarathonRunner.Core.Executors;

public class LocalExecutor : Executor
{
    public LocalExecutor(ILogger<LocalExecutor> logger) : base(logger)
    {
    }

    protected override Task PrepareFilesAsync(string problemName, IEnumerable<string> filePaths, CancellationToken ct = default)
    {
        // Do nothing
        return Task.CompletedTask;
    }
}