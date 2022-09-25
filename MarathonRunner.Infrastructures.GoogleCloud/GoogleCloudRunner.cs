using Microsoft.Extensions.Options;
using TerryU16.MarathonRunner.Core.Runners;

namespace TerryU16.MarathonRunner.Infrastructures.GoogleCloud;

public class GoogleCloudRunner : Runner<GoogleCloudDispatcher>
{
    public GoogleCloudRunner(GoogleCloudDispatcher dispatcher, IEnumerable<IRunnerCallback> callbacks, IOptions<RunnerOption> options) 
        : base(dispatcher, callbacks, options.Value.StartSeed, options.Value.EndSeed, options.Value.CloudParallelCount, options.Value.ResultDirectoryPath)
    {
    }
}