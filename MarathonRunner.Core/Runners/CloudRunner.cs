using Microsoft.Extensions.Options;
using TerryU16.MarathonRunner.Core.Dispatchers;

namespace TerryU16.MarathonRunner.Core.Runners;

public class CloudRunner : Runner<CloudDispatcher>
{
    public CloudRunner(CloudDispatcher dispatcher, IEnumerable<IRunnerCallback> callbacks, IOptions<RunnerOption> options) 
        : base(dispatcher, callbacks, options.Value.StartSeed, options.Value.EndSeed, options.Value.CloudParallelCount)
    {
    }
}