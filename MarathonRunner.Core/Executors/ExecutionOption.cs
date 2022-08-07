namespace TerryU16.MarathonRunner.Core.Executors;

public class ExecutionOption
{
    public string ScoreRegex { get; set; } = @"Score = (?<score>\d+)";
    public string SeedFormat { get; set; } = "0000";
    public string WorkingDirectory { get; set; } = ".";
    public string CloudFunctionKey { get; set; } = "";
    public Uri? CloudEndpoint { get; set; }
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(5);
    public ExecutionStep[] ExecutionSteps { get; set; } = Array.Empty<ExecutionStep>();
    public string[] Files { get; set; } = Array.Empty<string>();
}