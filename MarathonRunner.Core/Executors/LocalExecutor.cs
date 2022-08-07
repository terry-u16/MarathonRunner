using System.Diagnostics;
using System.Text.RegularExpressions;
using Cysharp.Diagnostics;
using Microsoft.Extensions.Logging;
using TerryU16.MarathonRunner.Core.Storage;
using TerryU16.MarathonRunner.Core.Executors;

namespace TerryU16.MarathonRunner.Executors.Local;

public class LocalExecutor : IExecutor
{
    private readonly ILogger<LocalExecutor> _logger;
    private readonly IDownloader _downloader;
    private readonly string _workingDirectory;

    public LocalExecutor(ILogger<LocalExecutor> logger, IDownloader downloader)
    {
        _logger = logger;
        _downloader = downloader;
    }

    public async Task<SingleCaseResult> ExecuteAsync(SingleCaseExecutorArgs args)
    {
        var regex = new Regex(args.ScoreRegex);
        double score = 0;
        var elapsed = new TimeSpan();
        var cancellationTokenSource = new CancellationTokenSource(args.Timeout);

        await DownloadFilesAsync(args.ProblemName, args.Files);

        foreach (var option in args.ExecutionOptions)
        {
            var (process, stdOut, stdError) = ProcessX.GetDualAsyncEnumerable(option.ExecutionCommand);

            var consumeStdOut = SetOutputCallback(stdOut, regex, option.StdOutPath, cancellationTokenSource.Token);
            var consumeStdError = SetOutputCallback(stdError, regex, option.StdErrorPath, cancellationTokenSource.Token);

            if (option.StdInPath is not null)
            {
                try
                {
                    var reader = new StreamReader(option.StdInPath);
                    await using var standardInput = process.StandardInput;
                    await process.StandardInput.WriteAsync(await reader.ReadToEndAsync());
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Exception raised: {Message}", ex.Message);
                }
            }

            var stopWatch = Stopwatch.StartNew();

            var scores = await Task.WhenAll(consumeStdOut, consumeStdError);
            stopWatch.Stop();

            score = scores.Prepend(score).Max();

            if (elapsed == default)
            {
                elapsed = stopWatch.Elapsed;
            }
        }

        return new SingleCaseResult(score, elapsed);
    }

    private async Task DownloadFilesAsync(string problemName, IEnumerable<string> filePaths)
    {
        foreach (var filePath in filePaths)
        {
            var destination = Path.Join(_workingDirectory, filePath);
            if (!File.Exists(destination))
            {
                await _downloader.DownloadFileAsync(problemName, filePath, destination);
            }
        }
    }

    private static Task<double> SetOutputCallback(ProcessAsyncEnumerable outputs, Regex regex,
        string? outputPath, CancellationToken cancellationToken = default)
    {
        var consumeOutput = Task.Run(async () =>
        {
            double score = 0;

            // 出力先がnullの場合はMemoryStreamにゴミを吐き出す
            var stream = outputPath is not null ? new StreamWriter(outputPath) : new StreamWriter(new MemoryStream());

            await foreach (var line in outputs.WithCancellation(cancellationToken))
            {
                var match = regex.Match(line);

                if (match.Success)
                {
                    score = double.Parse(match.Groups["score"].Value);
                }

                await stream.WriteLineAsync(line);
            }

            return score;
        }, cancellationToken);

        return consumeOutput;
    }
}