using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TerryU16.MarathonRunner.Core.Runners.Callbacks;

public class ScoreLogger : IRunnerCallback
{
    private long _totalScore;

    private int _completedCaseCount;

    private readonly object _lockObject;

    private readonly long _referenceScore;

    private readonly int _plannedCaseCount;

    private readonly ILogger<ScoreLogger> _logger;

    public ScoreLogger(ILogger<ScoreLogger> logger, IOptions<RunnerOption> options)
    {
        _totalScore = 0;
        _completedCaseCount = 0;
        _lockObject = new object();
        _referenceScore = options.Value.ReferenceScore;
        _plannedCaseCount = options.Value.EndSeed - options.Value.StartSeed;
        _logger = logger;
    }

    public void OnSingleTestEnd(int seed, TestCaseResult result)
    {
        lock (_lockObject)
        {
            _totalScore += result.Score;
            _completedCaseCount++;
            LogSingleResult(seed, result);
        }
    }

    public void OnAllTestEnd() => LogAllResult();

    protected virtual void LogSingleResult(int seed, TestCaseResult result)
    {
        var elapsed = (int)result.Elapsed.TotalMilliseconds;
        var rate = (double)result.Score * 100 / _referenceScore;
        var average = (double)_totalScore * 100 / (_referenceScore * _completedCaseCount);

        _logger.LogInformation("case {seed:00000} | completed: {count,5} / {cases,5} | {elapsed,5:#,##0} ms | {score,11:#,##0} | rate: {rate,7:0.000}% | average: {average,7:0.000}%",
            seed, _completedCaseCount, _plannedCaseCount, elapsed, result.Score, rate, average);
    }

    protected virtual void LogAllResult()
    {
        var average = (double)_totalScore * 100 / (_referenceScore * _completedCaseCount);
        _logger.LogInformation("[{DateTime:yyyy/MM/dd HH:mm:ss}] {Average,7:0.000}% | ", DateTime.Now, average);
    }

}
