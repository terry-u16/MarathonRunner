using Microsoft.Extensions.Logging;

namespace TerryU16.MarathonRunner.Core.Runners.Callbacks;

public class WrongAnswerCollector : SeedCollector
{
    protected override string Category => "WA";

    public WrongAnswerCollector(ILogger<WrongAnswerCollector> logger) : base(logger)
    {
    }

    protected override bool IsTarget(TestCaseResult result) => result.Score == 0;
}