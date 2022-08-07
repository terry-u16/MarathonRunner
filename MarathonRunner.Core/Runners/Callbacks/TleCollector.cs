using Microsoft.Extensions.Logging;

namespace TerryU16.MarathonRunner.Core.Runners.Callbacks;

public class TleCollector : SeedCollector
{
    private readonly TimeSpan _timeLimit;

    protected override string Category => "TLE";

    public TleCollector(ILogger<TleCollector> logger, TimeSpan timeLimit) : base(logger)
        => _timeLimit = timeLimit;

    protected override bool IsTarget(TestCaseResult result) => result.Elapsed > _timeLimit;
}