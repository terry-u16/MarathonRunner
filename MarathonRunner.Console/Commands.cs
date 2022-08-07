using TerryU16.MarathonRunner.Core.Runners;

namespace TerryU16.MarathonRunner.Console;

public class Commands : ConsoleAppBase
{
    private readonly LocalRunner _localRunner;
    private readonly CloudRunner _cloudRunner;

    public Commands(LocalRunner localRunner, CloudRunner cloudRunner)
    {
        _localRunner = localRunner;
        _cloudRunner = cloudRunner;
    }

    [Command("run-local", "ローカルでテストケースを実行します。")]
    public async Task RunLocalAsync() => await _localRunner.RunAsync(Context.CancellationToken);

    [Command("run-cloud", "クラウド上でテストケースを実行します。")]
    public async Task RunCloudAsync() => await _cloudRunner.RunAsync(Context.CancellationToken);
}