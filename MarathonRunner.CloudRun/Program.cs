using TerryU16.MarathonRunner.Core.Executors;
using TerryU16.MarathonRunner.Core.Storage;
using TerryU16.MarathonRunner.Infrastructures.GoogleCloud;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<IExecutor, LocalExecutor>();
builder.Services.AddTransient<IDownloader, GoogleCloudDownloader>();
builder.Logging.AddConsole();

builder.Host.ConfigureServices((context, services) =>
{
    var configurationRoot = context.Configuration;
    services.Configure<GoogleCloudOptions>(configurationRoot.GetSection(nameof(GoogleCloudOptions)));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/run", async (SingleCaseExecutorArgs? args, IExecutor executor) =>
{
    if (args is null)
    {
        return Results.BadRequest();
    }

    using var cts = new CancellationTokenSource(args.Timeout);

    var result = await executor.ExecuteAsync(args, cts.Token);
    return Results.Json(result);
})
.WithName("run");

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");