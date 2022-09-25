namespace TerryU16.MarathonRunner.Core.Executors;

public record SingleCaseExecutorArgs(string ProblemName, string ScoreRegex, int Seed, TimeSpan Timeout, ExecutionStep[] ExecutionSteps, string[] Files);