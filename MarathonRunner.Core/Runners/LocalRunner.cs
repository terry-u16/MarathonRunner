using Microsoft.Extensions.Options;
using TerryU16.MarathonRunner.Core.Dispatchers;

namespace TerryU16.MarathonRunner.Core.Runners;

public class LocalRunner : Runner<LocalDispatcher>
{
    public LocalRunner(LocalDispatcher dispatcher, IRunnerCallback[] callbacks, IOptions<RunnerOption> options) 
        : base(dispatcher, callbacks, options.Value.StartSeed, options.Value.EndSeed, options.Value.LocalParallelCount)
    {
    }
}