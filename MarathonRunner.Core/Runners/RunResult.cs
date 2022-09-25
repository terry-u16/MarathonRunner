namespace TerryU16.MarathonRunner.Core.Runners;

public record RunResult(DateTimeOffset TimeStamp, string Comment, TestCaseResult[] Results);