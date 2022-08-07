﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using TerryU16.MarathonRunner.Core;
using TerryU16.MarathonRunner.Core.Dispatchers;
using TerryU16.MarathonRunner.Core.Executors;
using TerryU16.MarathonRunner.Core.Runners;
using TerryU16.MarathonRunner.Core.Runners.Callbacks;
using TerryU16.MarathonRunner.Core.Storage;
using TerryU16.MarathonRunner.Executors.Local;
using TerryU16.MarathonRunner.Infrastructures.AzureBlobStorage;

namespace TerryU16.MarathonRunner.Console;

public static class ConsoleAppBuilderExtensions
{
    public static ConsoleAppBuilder ConfigureOptions(this ConsoleAppBuilder builder)
    {
        // https://docs.microsoft.com/ja-jp/dotnet/core/extensions/options
        return builder.ConfigureAppConfiguration(configuration =>
        {
            configuration.AddJsonFile(Constants.ConfigurationFileName, true);
        });
    }

    public static ConsoleAppBuilder ConfigureServiceDependencies(this ConsoleAppBuilder builder)
    {
        return builder.ConfigureServices((context, services) =>
        {
            // DI
            services.AddTransient<Runner<LocalDispatcher>>();
            services.AddTransient<Runner<CloudDispatcher>>();
            services.AddTransient<IRunnerCallback, ScoreLogger>();
            services.AddTransient<IRunnerCallback, SummaryLogger>();
            services.AddTransient<IRunnerCallback, TleCollector>();
            services.AddTransient<IRunnerCallback, WrongAnswerCollector>();
            services.AddTransient<LocalDispatcher>();
            services.AddTransient<CloudDispatcher>();
            services.AddTransient<IExecutor, LocalExecutor>();
            services.AddTransient<IDownloader, BlobDownloader>();

            // Options
            var configurationRoot = context.Configuration;
            services.Configure<ProblemOption>(configurationRoot.GetSection(nameof(ProblemOption)));
            services.Configure<RunnerOption>(configurationRoot.GetSection(nameof(RunnerOption)));
            services.Configure<ExecutionOption>(configurationRoot.GetSection(nameof(ExecutionOption)));
            services.Configure<BlobClientOption>(configurationRoot.GetSection(nameof(BlobClientOption)));
            
            // HttpClient
            services.AddHttpClient<CloudDispatcher>((s, client) =>
            {
                var functionKey = s.GetRequiredService<IOptions<ExecutionOption>>().Value.CloudFunctionKey;
                client.DefaultRequestHeaders.Add("x-functions-key", functionKey);
            }).AddPolicyHandler(GetRetryPolicy());
        });
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        var jitterier = Random.Shared;
        return HttpPolicyExtensions.HandleTransientHttpError()
            .WaitAndRetryAsync(10, _ => TimeSpan.FromSeconds(2) + TimeSpan.FromMilliseconds(jitterier.Next(0, 500)));
    }
}