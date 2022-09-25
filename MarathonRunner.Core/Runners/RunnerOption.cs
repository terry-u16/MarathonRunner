namespace TerryU16.MarathonRunner.Core.Runners;

public class RunnerOption
{
    public int StartSeed { get; set; } = 0;

    public int EndSeed { get; set; } = 1024;

    public long ReferenceScore { get; set; } = 100;

    public int LocalParallelCount { get; set; } = Environment.ProcessorCount;

    public int CloudParallelCount { get; set; } = 300;

    public string SummaryFilePath { get; set; } = @".\data\score_history.txt";

    public string ResultDirectoryPath { get; set; } = @".\data\results";
}