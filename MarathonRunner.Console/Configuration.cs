using TerryU16.MarathonRunner.Core;
using TerryU16.MarathonRunner.Core.Executors;
using TerryU16.MarathonRunner.Core.Runners;
using TerryU16.MarathonRunner.Infrastructures.AzureBlobStorage;

namespace TerryU16.MarathonRunner.Console;

public record Configuration(ProblemOption ProblemOption, RunnerOption RunnerOption,
    ExecutionOption ExecutionOption, BlobClientOption BlobClientOption);