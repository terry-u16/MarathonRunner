using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TerryU16.MarathonRunner.Core.Executors;

namespace TerryU16.MarathonRunner.Core.Dispatchers;

public class LocalDispatcher : Dispatcher
{
    private readonly IExecutor _executor;
    
    public LocalDispatcher(IOptions<ProblemOption> problemOptions, IOptions<ExecutionOption> executionOptions, 
        ILogger<LocalDispatcher> logger, IExecutor executor) : base(problemOptions, executionOptions, logger, 
        executionOptions.Value.LocalExecutionSteps)
    {
        _executor = executor;
    }

    protected override async Task<TestCaseResult> DispatchAsyncInner(SingleCaseExecutorArgs args, CancellationToken ct = default) 
        => await _executor.ExecuteAsync(args, ct);
}