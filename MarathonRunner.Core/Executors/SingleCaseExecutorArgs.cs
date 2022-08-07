using TerryU16.MarathonRunner.Core.Storage;

namespace TerryU16.MarathonRunner.Core.Executors;

public record SingleCaseExecutorArgs(string ProblemName, string ScoreRegex, TimeSpan Timeout, ExecutionOption[] ExecutionOptions, string[] Files);