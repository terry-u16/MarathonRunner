using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TerryU16.MarathonRunner.Core.Executors;

namespace TerryU16.MarathonRunner.Core.Dispatchers;

public abstract class Dispatcher : IDispatcher
{
    private readonly string _problemName;
    private readonly string _scoreRegex;
    private readonly string _seedFormat;
    private readonly TimeSpan _timeout;
    private readonly ExecutionStep[] _executionSteps;
    private readonly string[] _files;
    private readonly ILogger<Dispatcher> _logger;

    protected Dispatcher(IOptions<ProblemOption> problemOptions, IOptions<ExecutionOption> executionOptions, ILogger<Dispatcher> logger, ExecutionStep[] executionSteps)
    {
        _logger = logger;
        _problemName = problemOptions.Value.ProblemName;
        _scoreRegex = executionOptions.Value.ScoreRegex;
        _seedFormat = executionOptions.Value.SeedFormat;
        _timeout = executionOptions.Value.Timeout;
        _files = executionOptions.Value.Files;
        _executionSteps = executionSteps;
    }

    public async Task<TestCaseResult> DispatchAsync(int seed, CancellationToken ct = default)
    {
        try
        {
            var steps = _executionSteps.Select(step => step.WithSeed(seed, _seedFormat)).ToArray();
            var files = _files.Select(f => f.Replace(ExecutionOption.SeedPlaceholder, seed.ToString(_seedFormat)))
                .ToArray();
            var args = new SingleCaseExecutorArgs(_problemName, _scoreRegex, _timeout, steps, files);
            var result = await DispatchAsyncInner(args, ct);
            if (!string.IsNullOrWhiteSpace(result.Message))
            {
                _logger.LogWarning(result.Message);
            }
            return result;
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex.Message);
            return new(0, default, ex.Message);
        }
    }

    protected abstract Task<TestCaseResult> DispatchAsyncInner(SingleCaseExecutorArgs args, CancellationToken ct = default);
}