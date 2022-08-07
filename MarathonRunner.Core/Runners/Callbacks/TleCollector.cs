using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TerryU16.MarathonRunner.Core.Runners.Callbacks;

public class TleCollector : SeedCollector
{
    private readonly TimeSpan _timeLimit;

    protected override string Category => "TLE";

    public TleCollector(IOptions<ProblemOption> options, ILogger<TleCollector> logger) : base(logger)
        => _timeLimit = options.Value.TimeLimit;

    protected override bool IsTarget(TestCaseResult result) => result.Elapsed > _timeLimit;
}