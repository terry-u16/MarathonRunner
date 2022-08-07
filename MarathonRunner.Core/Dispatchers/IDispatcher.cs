namespace TerryU16.MarathonRunner.Core.Dispatchers;

public interface IDispatcher
{
    Task<TestCaseResult> DispatchAsync(int seed, CancellationToken ct = default);
}