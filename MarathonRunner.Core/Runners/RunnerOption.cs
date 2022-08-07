namespace TerryU16.MarathonRunner.Core.Runners;

public class RunnerOption
{
    public int StartSeed { get; set; } = 0;
    public int EndSeed { get; set; } = 1024;
    public long ReferenceScore { get; set; } = 100;
    public int ParallelCount { get; set; } = 1;
}