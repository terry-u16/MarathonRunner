namespace TerryU16.MarathonRunner.Core;

public record TestCaseResult(int Seed, long Score, TimeSpan Elapsed, string Message = "");