using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using TerryU16.MarathonRunner.Core;
using TerryU16.MarathonRunner.Core.Compilers;
using TerryU16.MarathonRunner.Core.Dispatchers;
using TerryU16.MarathonRunner.Core.Executors;
using TerryU16.MarathonRunner.Core.Runners;
using TerryU16.MarathonRunner.Core.Runners.Callbacks;
using TerryU16.MarathonRunner.Core.Storage;
using TerryU16.MarathonRunner.Infrastructures.GoogleCloud;
using TerryU16.MarathonRunner.Infrastructures.GoogleCloud.Compilers;

namespace TerryU16.MarathonRunner.Console;

public static class ConsoleAppBuilderExtensions
{
    public static ConsoleAppBuilder ConfigureOptions(this ConsoleAppBuilder builder)
    {
        // https://docs.microsoft.com/ja-jp/dotnet/core/extensions/options
        return builder.ConfigureAppConfiguration(configuration =>
        {
            configuration.AddJsonFile(Constants.ConfigurationFileName, true);
            configuration.AddUserSecrets("d964bdd1-3335-4c95-b108-626596194283");
        });
    }

    public static ConsoleAppBuilder ConfigureServiceDependencies(this ConsoleAppBuilder builder)
    {
        return builder.ConfigureServices((context, services) =>
        {
            // DI
            services.AddTransient<LocalRunner>();
            services.AddTransient<GoogleCloudRunner>();
            services.AddTransient<IRunnerCallback, ScoreLogger>();
            services.AddTransient<IRunnerCallback, SummaryLogger>();
            services.AddTransient<IRunnerCallback, TleCollector>();
            services.AddTransient<IRunnerCallback, WrongAnswerCollector>();
            services.AddTransient<LocalDispatcher>();
            services.AddTransient<GoogleCloudDispatcher>();
            services.AddTransient<RustCompileDispatcher>();
            services.AddTransient<IExecutor, LocalExecutor>();
            services.AddTransient<IDownloader, GoogleCloudDownloader>();

            // Options
            var configurationRoot = context.Configuration;
            services.Configure<ProblemOption>(configurationRoot.GetSection(nameof(ProblemOption)));
            services.Configure<RunnerOption>(configurationRoot.GetSection(nameof(RunnerOption)));
            services.Configure<ExecutionOption>(configurationRoot.GetSection(nameof(ExecutionOption)));
            services.Configure<CompileOption>(configurationRoot.GetSection(nameof(CompileOption)));
            services.Configure<GoogleCloudOptions>(configurationRoot.GetSection(nameof(GoogleCloudOptions)));

            // HttpClient
            services.AddHttpClient<GoogleCloudDispatcher>().AddPolicyHandler(GetRetryPolicy());
        });
    }

    public static ConsoleAppBuilder ConfigureLogging(this ConsoleAppBuilder builder)
    {
        return builder.ConfigureLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Information);
        });
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        var jitterier = Random.Shared;
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(10, _ => TimeSpan.FromMilliseconds(1000) + TimeSpan.FromMilliseconds(jitterier.Next(0, 200)));
    }
}