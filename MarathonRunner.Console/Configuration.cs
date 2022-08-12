using TerryU16.MarathonRunner.Core;
using TerryU16.MarathonRunner.Core.Compilers;
using TerryU16.MarathonRunner.Core.Executors;
using TerryU16.MarathonRunner.Core.Runners;

namespace TerryU16.MarathonRunner.Console;

public record Configuration(ProblemOption ProblemOption, RunnerOption RunnerOption,
    ExecutionOption ExecutionOption, CompileOption CompileOption);