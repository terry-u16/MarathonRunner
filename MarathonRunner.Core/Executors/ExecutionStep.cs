namespace TerryU16.MarathonRunner.Core.Executors;

public record ExecutionStep(string ExecutionCommand, string? StdInPath, string? StdOutPath, string? StdErrorPath)
{
    private const string SeedPlaceholder = "{SEED}";

    public ExecutionStep WithSeed(int seed, string seedFormat)
    {
        return new(
            ExecutionCommand.Replace(SeedPlaceholder, seed.ToString(seedFormat)),
            StdInPath?.Replace(SeedPlaceholder, seed.ToString(seedFormat)),
            StdOutPath?.Replace(SeedPlaceholder, seed.ToString(seedFormat)),
            StdErrorPath?.Replace(SeedPlaceholder, seed.ToString(seedFormat))
        );
    }
}