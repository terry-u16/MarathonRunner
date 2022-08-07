namespace TerryU16.MarathonRunner.Core.Executors;

public interface IExecutor
{
    Task<SingleCaseResult> ExecuteAsync(SingleCaseExecutorArgs args);
}