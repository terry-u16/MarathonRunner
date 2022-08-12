using TerryU16.MarathonRunner.Console;

var builder = ConsoleApp.CreateBuilder(args, options =>
{
    options.ApplicationName = "marathon-runner";
});
var app = builder.ConfigureOptions().ConfigureServiceDependencies().ConfigureLogging().Build();
app.AddCommands<Commands>();
app.Run();
