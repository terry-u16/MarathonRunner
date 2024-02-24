using System.Diagnostics;
using Cysharp.Diagnostics;
using TerryU16.MarathonRunner.Core;
using TerryU16.MarathonRunner.Core.Executors;
using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Storage.v1.Data;
using Google.Cloud.Storage.V1;
using TerryU16.MarathonRunner.Infrastructures.GoogleCloud.Compilers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.MapPost("/", async (RustCompileArgs? args) =>
    {
        if (args is null)
        {
            return Results.BadRequest();
        }

        const string tempDirectory = "temp";

        try
        {
            // �R�s�[
            Directory.CreateDirectory(tempDirectory);
            var copyTasks = CopyDirectory("work", "temp", true);
            await Task.WhenAll(copyTasks);

            foreach (var file in args.Files)
            {
                var path = Path.Combine(tempDirectory, file.FilePath);
                var parentDirectory = Directory.GetParent(path);
                if (!parentDirectory?.Exists ?? true)
                {
                    parentDirectory?.Create();
                }
                
                await using var writer = new StreamWriter(new FileStream(path, FileMode.Create, FileAccess.Write));
                await writer.WriteAsync(file.Content);
            }

            // �r���h
            var (_, stdOut, stdErr) = ProcessX.GetDualAsyncEnumerable("cargo build --release --bin main", workingDirectory: tempDirectory);
            var consumeStdOut = Task.Run(async () =>
            {
                await foreach (var item in stdOut)
                {
                    Console.WriteLine("STD_OUT: " + item);
                }
            });

            var consumeStdError = Task.Run(async () =>
            {
                await foreach (var item in stdErr)
                {
                    Console.WriteLine("STD_ERROR: " + item);
                }
            });

            await Task.WhenAll(consumeStdOut, consumeStdError);

            var credential = await GoogleCredential.GetApplicationDefaultAsync();
            var storage = await StorageClient.CreateAsync(credential);
            await using var stream = new FileStream("temp/target/release/main", FileMode.Open, FileAccess.Read);
            var contentPath = $"{args.ContestName}/{args.ExeName}";
            await storage.UploadObjectAsync(args.BucketName, contentPath, "application/octet-stream", stream);
            
            return Results.Ok();
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, true);
            }
        }
    });

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");

static List<Task> CopyDirectory(string sourceDir, string destinationDir, bool recursive)
{
    var dir = new DirectoryInfo(sourceDir);

    if (!dir.Exists)
        throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

    Directory.CreateDirectory(destinationDir);

    var tasks = new List<Task>();

    foreach (var file in dir.EnumerateFiles())
    {
        var task = Task.Run(() =>
        {
            file.CopyTo(Path.Combine(destinationDir, file.Name));
        });
        tasks.Add(task);
    }

    if (recursive)
    {
        foreach (var subDir in dir.EnumerateDirectories())
        {
            var newDestinationDir = Path.Combine(destinationDir, subDir.Name);
            tasks.AddRange(CopyDirectory(subDir.FullName, newDestinationDir, true));
        }
    }

    return tasks;
}