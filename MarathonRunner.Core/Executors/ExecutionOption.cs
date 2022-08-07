namespace TerryU16.MarathonRunner.Core.Executors;

public record ExecutionOption(string ExecutionCommand, string? StdInPath, string? StdOutPath, string? StdErrorPath);