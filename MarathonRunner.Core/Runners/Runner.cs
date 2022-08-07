using Microsoft.Extensions.Options;
using TerryU16.MarathonRunner.Core.Dispatchers;

namespace TerryU16.MarathonRunner.Core.Runners;

public abstract class Runner<T> where T : IDispatcher
{
    private readonly int _startSeed;
    private readonly int _endSeed;
    private readonly int _parallelCount;
    private readonly T _dispatcher;
    private readonly IRunnerCallback[] _callbacks;

    private protected Runner(T dispatcher, IEnumerable<IRunnerCallback> callbacks, int startSeed, int endSeed, int parallelCount)
    {
        _dispatcher = dispatcher;
        _callbacks = callbacks.ToArray();
        _startSeed = startSeed;
        _endSeed = endSeed;
        _parallelCount = parallelCount;
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var option = new ParallelOptions
        {
            MaxDegreeOfParallelism = _parallelCount,
            CancellationToken = ct
        };

        var seeds = Enumerable.Range(_startSeed, _endSeed - _startSeed);

        await Parallel.ForEachAsync(seeds, option, async (seed, innerCt) =>
        {
            var result = await _dispatcher.DispatchAsync(seed, innerCt);
            foreach (var callback in _callbacks)
            {
                callback.OnSingleTestEnd(seed, result);
            }
        });

        foreach (var callback in _callbacks)
        {
            callback.OnAllTestEnd();
        }
    }
}