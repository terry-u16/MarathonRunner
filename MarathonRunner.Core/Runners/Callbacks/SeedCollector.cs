using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace TerryU16.MarathonRunner.Core.Runners.Callbacks;

public abstract class SeedCollector : IRunnerCallback
{
    private readonly ConcurrentBag<int> _seeds;
    private readonly ILogger<SeedCollector> _logger;

    protected SeedCollector(ILogger<SeedCollector> logger)
    {
        _logger = logger;
        _seeds = new ConcurrentBag<int>();
    }

    protected abstract string Category { get; }

    protected abstract bool IsTarget(TestCaseResult result);

    public void OnSingleTestEnd(int seed, TestCaseResult result)
    {
        if (IsTarget(result))
        {
            _seeds.Add(seed);
        }
    }

    public void OnAllTestEnd()
    {
        var seeds = string.Join(',', _seeds.OrderBy(s => s).Select(s => s.ToString()));
        _logger.LogInformation("{category} seeds: {seeds}", Category, seeds);
    }
}