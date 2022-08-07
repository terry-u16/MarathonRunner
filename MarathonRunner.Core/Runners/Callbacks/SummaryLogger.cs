namespace TerryU16.MarathonRunner.Core.Runners.Callbacks;

public class SummaryLogger : IRunnerCallback
{
    private readonly string _filename;
    private readonly long _referenceScore;
    private long _totalScore;
    private int _completedCaseCount;

    public SummaryLogger(string filename, long referenceScore)
    {
        _filename = filename;
        _referenceScore = referenceScore;
        _totalScore = 0;
        _completedCaseCount = 0;
    }

    public void OnSingleTestEnd(int seed, TestCaseResult result)
    {
        Interlocked.Add(ref _totalScore, result.Score);
        Interlocked.Increment(ref _completedCaseCount);
    }

    public void OnAllTestEnd()
    {
        using var writer = CreateLogFileWriter();

        var average = (double)_totalScore * 100 / (_referenceScore * _completedCaseCount);
        var endTime = DateTimeOffset.Now;
        writer.WriteLine($"[{endTime:yyyy/MM/dd HH:mm:ss}] {_completedCaseCount,6:#,##0} cases | {_totalScore,17:#,##0} | {average,7:0.000}% | ");
    }

    private StreamWriter CreateLogFileWriter()
    {
        var directory = Path.GetDirectoryName(_filename);

        if (directory is not null && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        return new StreamWriter(_filename, true);
    }
}