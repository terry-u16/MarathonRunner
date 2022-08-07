using System.Text.Encodings.Web;
using System.Text.Json;
using TerryU16.MarathonRunner.Core.Executors;
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

    [Command("init", "ツールの初期化を行います。")]
    public async Task InitializeAsync()
    {
        if (File.Exists(Constants.ConfigurationFileName))
        {
            string? result = null;
            while (result != "y" && result != "n")
            {
                System.Console.WriteLine($"{Constants.ConfigurationFileName}を上書きします。よろしいですか？[y/n]");
                result = System.Console.ReadLine()?.ToLower();
            }
        }

        var problemName = ShowInputPrompt("問題名");
        var timeLimit = TimeSpan.FromSeconds(double.Parse(ShowInputPrompt("実行時間制限[s]")));
        var caseCount = int.Parse(ShowInputPrompt("実行ケース数"));
        var configuration = new Configuration(new (), new (), new (), new ())
        {
            ProblemOption =
            {
                ProblemName = problemName,
                TimeLimit = timeLimit
            },
            RunnerOption =
            {
                EndSeed = caseCount
            },
            ExecutionOption =
            {
                Timeout = timeLimit * 2,
                ExecutionSteps = new[]
                {
                    new ExecutionStep
                    {
                        ExecutionCommand = "a.exe",
                        StdInPath = "in/{SEED}.txt"
                    }
                },
                Files = new[] { "in/{SEED}.txt" }
            }
        };

        var serializerOptions = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true
        };
        await using var stream = new FileStream(Constants.ConfigurationFileName, FileMode.Create, FileAccess.Write);
        await JsonSerializer.SerializeAsync(stream, configuration, serializerOptions, Context.CancellationToken);
    }

    private static string ShowInputPrompt(string itemName)
    {
        System.Console.Write($"{itemName} : ");
        var input = System.Console.ReadLine();
        if (input is null) throw new ObjectDisposedException("StandardInput");
        return input;
    }

    [Command("run-local", "ローカルでテストケースを実行します。")]
    public async Task RunLocalAsync()
    {
        await _localRunner.RunAsync(Context.CancellationToken);
    }

    [Command("run-cloud", "クラウド上でテストケースを実行します。")]
    public async Task RunCloudAsync()
    {
        await _cloudRunner.RunAsync(Context.CancellationToken);
    }
}