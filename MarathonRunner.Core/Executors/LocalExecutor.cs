using System.Diagnostics;
using System.Text.RegularExpressions;
using Cysharp.Diagnostics;
using Microsoft.Extensions.Logging;
using TerryU16.MarathonRunner.Core.Storage;

namespace TerryU16.MarathonRunner.Core.Executors;

public class LocalExecutor : IExecutor
{
    private readonly ILogger<LocalExecutor> _logger;
    private readonly IDownloader _downloader;

    public LocalExecutor(ILogger<LocalExecutor> logger, IDownloader downloader)
    {
        _logger = logger;
        _downloader = downloader;
    }

    public async Task<TestCaseResult> ExecuteAsync(SingleCaseExecutorArgs args, CancellationToken ct = default)
    {
        try
        {
            var regex = new Regex(args.ScoreRegex);
            long score = 0;
            var elapsed = new TimeSpan();
            using var cts = new CancellationTokenSource(args.Timeout);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, cts.Token);

            await DownloadFilesAsync(args.ProblemName, args.Files, ct);

            foreach (var option in args.ExecutionSteps)
            {
                var (process, stdOut, stdError) = ProcessX.GetDualAsyncEnumerable(option.ExecutionCommand);

                var consumeStdOut = SetOutputCallback(stdOut, regex, option.StdOutPath, linkedCts.Token);
                var consumeStdError = SetOutputCallback(stdError, regex, option.StdErrorPath, linkedCts.Token);

                if (!string.IsNullOrWhiteSpace(option.StdInPath))
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

            return new TestCaseResult(score, elapsed);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex.Message);
            return new TestCaseResult(0, default, ex.Message);
        }
    }

    private async Task DownloadFilesAsync(string problemName, IEnumerable<string> filePaths, CancellationToken ct = default)
    {
        foreach (var filePath in filePaths)
        {
            if (!File.Exists(filePath))
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

    private static Task<long> SetOutputCallback(ProcessAsyncEnumerable outputs, Regex regex,
        string? outputPath, CancellationToken cancellationToken = default)
    {
        var consumeOutput = Task.Run(async () =>
        {
            long score = 0;

            // 出力先がnullの場合はMemoryStreamにゴミを吐き出す
            await using var stream = !string.IsNullOrWhiteSpace(outputPath) ? new StreamWriter(outputPath) : new StreamWriter(new MemoryStream());

            await foreach (var line in outputs.WithCancellation(cancellationToken))
            {
                var match = regex.Match(line);

                if (match.Success)
                {
                    score = long.Parse(match.Groups["score"].Value);
                }

                await stream.WriteLineAsync(line);
            }

            return score;
        }, cancellationToken);

        return consumeOutput;
    }
}