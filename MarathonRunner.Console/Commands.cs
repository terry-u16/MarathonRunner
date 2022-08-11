using System.Text.Encodings.Web;
using System.Text.Json;
using TerryU16.MarathonRunner.Core.Compilers;
using TerryU16.MarathonRunner.Core.Executors;
using TerryU16.MarathonRunner.Core.Runners;
using TerryU16.MarathonRunner.Infrastructures.GoogleCloud;
using TerryU16.MarathonRunner.Infrastructures.GoogleCloud.Compilers;

namespace TerryU16.MarathonRunner.Console;

public class Commands : ConsoleAppBase
{
    private readonly LocalRunner _localRunner;
    private readonly GoogleCloudRunner _cloudRunner;
    private readonly RustCompileDispatcher _rustCompileDispatcher;

    public Commands(LocalRunner localRunner, GoogleCloudRunner cloudRunner, RustCompileDispatcher rustCompileDispatcher)
    {
        _localRunner = localRunner;
        _cloudRunner = cloudRunner;
        _rustCompileDispatcher = rustCompileDispatcher;
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

            if (result == "n")
            {
                return;
            }
        }

        var problemName = ShowInputPrompt("問題名");
        var timeLimit = TimeSpan.FromSeconds(double.Parse(ShowInputPrompt("実行時間制限[s]")));
        var caseCount = int.Parse(ShowInputPrompt("実行ケース数"));
        var scoreRegex = ShowInputPrompt("スコア正規表現");
        var referenceScore = long.Parse(ShowInputPrompt("レファレンススコア"));
        var configuration = new Configuration(new (), new (), new (), new ())
        {
            ProblemOption =
            {
                ProblemName = problemName,
                TimeLimit = timeLimit,
            },
            RunnerOption =
            {
                EndSeed = caseCount,
                ReferenceScore = referenceScore
            },
            ExecutionOption =
            {
                ScoreRegex = scoreRegex,
                Timeout = timeLimit * 2,
                LocalExecutionSteps = new[]
                {
                    new ExecutionStep
                    {
                        ExecutionCommand = $"{problemName}-a.exe",
                        StdInPath = "in/{SEED}.txt"
                    }
                },
                CloudExecutionSteps = new[]
                {
                    new ExecutionStep
                    {
                        ExecutionCommand = "main",
                        StdInPath = "in/{SEED}.txt"
                    }
                },
                Files = new[] { "in/{SEED}.txt" }
            },
            CompileOption =
            {
                ExeName = "main",
                Files = new[]
                {
                    new CompileFile
                    {
                        Source = "src/bin/a.rs",
                        Destination = "src/bin/main.rs"
                    }
                }
            }
        };

        var serializerOptions = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true
        };
        await using var stream = new FileStream(Constants.ConfigurationFileName, FileMode.Create, FileAccess.Write);
        await JsonSerializer.SerializeAsync(stream, configuration, serializerOptions, Context.CancellationToken);
        System.Console.WriteLine("Initialized!");
    }

    private static string ShowInputPrompt(string itemName)
    {
        System.Console.Write($"{itemName} : ");
        var input = System.Console.ReadLine();
        if (input is null) throw new ObjectDisposedException("StandardInput");
        return input;
    }

    [Command("compile-rust", "Rustのコンパイルを実行します。")]
    public async Task CompileRustAsync()
    {
        await _rustCompileDispatcher.CompileAsync(Context.CancellationToken);
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