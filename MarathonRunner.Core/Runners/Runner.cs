using System.Collections.Concurrent;
using System.Text.Json;
using TerryU16.MarathonRunner.Core.Dispatchers;

namespace TerryU16.MarathonRunner.Core.Runners;

public abstract class Runner<T> where T : IDispatcher
{
    private readonly int _startSeed;
    private readonly int _endSeed;
    private readonly int _parallelCount;
    private readonly T _dispatcher;
    private readonly IRunnerCallback[] _callbacks;
    private readonly string _resultDirectoryPath;

    protected Runner(T dispatcher, IEnumerable<IRunnerCallback> callbacks, int startSeed, int endSeed, int parallelCount, string resultDirectoryPath)
    {
        _dispatcher = dispatcher;
        _callbacks = callbacks.ToArray();
        _startSeed = startSeed;
        _endSeed = endSeed;
        _parallelCount = parallelCount;
        _resultDirectoryPath = resultDirectoryPath;
    }

    public async Task RunAsync(string comment = "", CancellationToken ct = default)
    {
        var timeStamp = DateTimeOffset.Now;

        var option = new ParallelOptions
        {
            MaxDegreeOfParallelism = _parallelCount,
            CancellationToken = ct
        };

        var seeds = Enumerable.Range(_startSeed, _endSeed - _startSeed);
        var results = new ConcurrentBag<TestCaseResult>();

        await Parallel.ForEachAsync(seeds, option, async (seed, innerCt) =>
        {
            var result = await _dispatcher.DispatchAsync(seed, innerCt);
            foreach (var callback in _callbacks)
            {
                callback.OnSingleTestEnd(seed, result);
            }

            results.Add(result);
        });

        foreach (var callback in _callbacks)
        {
            callback.OnAllTestEnd();
        }

        var runResult = new RunResult(timeStamp, comment, results.OrderBy(result => result.Seed).ToArray());
        await SaveResultsAsync(runResult);
    }

    public async Task SaveResultsAsync(RunResult result)
    {
        if (!Directory.Exists(_resultDirectoryPath))
        {
            Directory.CreateDirectory(_resultDirectoryPath);
        }

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        var dateTimeString = result.TimeStamp.ToString("yyyyMMdd_HHmmss");
        var fileName = $"result_{dateTimeString}.json";
        await using var stream = new FileStream(Path.Join(_resultDirectoryPath, fileName), FileMode.Create, FileAccess.Write);
        await JsonSerializer.SerializeAsync(stream, result, options);
    }
}