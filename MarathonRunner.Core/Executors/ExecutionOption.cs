namespace TerryU16.MarathonRunner.Core.Executors;

public class ExecutionOption
{
    public const string SeedPlaceholder = "{SEED}";

    public string ScoreRegex { get; set; } = @"Score = (?<score>\d+)";

    public string SeedFormat { get; set; } = "0000";

    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(5);

    public ExecutionStep[] LocalExecutionSteps { get; set; } = Array.Empty<ExecutionStep>();

    public ExecutionStep[] CloudExecutionSteps { get; set; } = Array.Empty<ExecutionStep>();

    public string[] Files { get; set; } = Array.Empty<string>();
}