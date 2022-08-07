namespace TerryU16.MarathonRunner.Core.Executors;

public interface IExecutor
{
    Task<TestCaseResult> ExecuteAsync(SingleCaseExecutorArgs args, CancellationToken ct = default);
}