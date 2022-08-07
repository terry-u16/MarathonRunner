namespace TerryU16.MarathonRunner.Core.Executors;

public class ExecutionStep
{
    private const string SeedPlaceholder = "{SEED}";
    public string ExecutionCommand { get; set; } = "a.exe";
    public string? StdInPath { get; set; } = null;
    public string? StdOutPath { get; set; } = null;
    public string? StdErrorPath { get; set; } = null;

    public ExecutionStep WithSeed(int seed, string seedFormat)
    {
        return new()
        {
            ExecutionCommand = ExecutionCommand.Replace(SeedPlaceholder, seed.ToString(seedFormat)),
            StdInPath = StdInPath?.Replace(SeedPlaceholder, seed.ToString(seedFormat)),
            StdOutPath = StdOutPath?.Replace(SeedPlaceholder, seed.ToString(seedFormat)),
            StdErrorPath = StdErrorPath?.Replace(SeedPlaceholder, seed.ToString(seedFormat))
        };
    }
}