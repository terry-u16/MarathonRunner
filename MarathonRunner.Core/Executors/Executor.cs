using System.Diagnostics;
using System.Text.RegularExpressions;
using Cysharp.Diagnostics;
using Microsoft.Extensions.Logging;

namespace TerryU16.MarathonRunner.Core.Executors;

public abstract class Executor : IExecutor
{
    private readonly ILogger<Executor> _logger;

    protected Executor(ILogger<Executor> logger)
    {
        _logger = logger;
    }

    public async Task<TestCaseResult> ExecuteAsync(SingleCaseExecutorArgs args, CancellationToken ct = default)
    {
        var messages = new List<string>();

        try
        {
            var regex = new Regex(args.ScoreRegex);
            long score = 0;
            var elapsed = new TimeSpan();
            using var cts = new CancellationTokenSource(args.Timeout);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, cts.Token);

            await PrepareFilesAsync(args.ProblemName, args.Files, ct);

            foreach (var option in args.ExecutionSteps)
            {
                var (process, stdOut, stdError) = ProcessX.GetDualAsyncEnumerable(option.ExecutionCommand);

                var consumeStdOut = SetOutputCallback(stdOut, regex, option.StdOutPath, messages, linkedCts.Token);
                var consumeStdError = SetOutputCallback(stdError, regex, option.StdErrorPath, messages, linkedCts.Token);

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

            return new TestCaseResult(args.Seed, score, elapsed);
        }
        catch (Exception ex)
        {
            var message = string.Join(Environment.NewLine, messages.Prepend(ex.Message));
            _logger.LogWarning("{Message}", message);
            return new TestCaseResult(args.Seed, 0, default, message);
        }
    }

    protected abstract Task PrepareFilesAsync(string problemName, IEnumerable<string> filePaths,
        CancellationToken ct = default);

    private static Task<long> SetOutputCallback(ProcessAsyncEnumerable outputs, Regex regex,
        string? outputPath, List<string> messages, CancellationToken cancellationToken = default)
    {
        var consumeOutput = Task.Run(async () =>
        {
            long score = 0;

            // 出力先がnullの場合はMemoryStreamにゴミを吐き出す
            await using var stream = !string.IsNullOrWhiteSpace(outputPath) ? new StreamWriter(outputPath) : new StreamWriter(new MemoryStream());

            await foreach (var line in outputs.WithCancellation(cancellationToken))
            {
                messages.Add(line);
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