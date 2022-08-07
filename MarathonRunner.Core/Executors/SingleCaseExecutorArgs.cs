namespace TerryU16.MarathonRunner.Core.Executors;

public record SingleCaseExecutorArgs(string ProblemName, string ScoreRegex, TimeSpan Timeout, ExecutionStep[] ExecutionSteps, string[] Files);