using System.Diagnostics;
using TerryU16.MarathonRunner.Core;
using TerryU16.MarathonRunner.Core.Executors;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/run", (SingleCaseExecutorArgs? args) =>
{
    if (args is null)
    {
        return Results.BadRequest();
    }

    var sw = Stopwatch.StartNew();
    long counter = 0;
    var timeLimit = TimeSpan.FromSeconds(2);

    while (true)
    {
        if ((counter++ & ((1 << 20) - 1)) == 0 && sw.Elapsed > timeLimit)
        {
            break;
        }
    }

    var result = new TestCaseResult(counter, sw.Elapsed);
    return Results.Json(result);
})
.WithName("run");

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");