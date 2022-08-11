using System.Diagnostics;
using Cysharp.Diagnostics;
using TerryU16.MarathonRunner.Core;
using TerryU16.MarathonRunner.Core.Executors;
using TerryU16.MarathonRunner.Infrastructures.GoogleCloud;
using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Storage.v1.Data;
using Google.Cloud.Storage.V1;

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
            // コピー
            Directory.CreateDirectory(tempDirectory);
            CopyDirectory("work", "temp", true);

            foreach (var file in args.Files)
            {
                var path = Path.Combine(tempDirectory, file.FilePath);
                var parentDirectory = Directory.GetParent(path);
                if (!parentDirectory?.Exists ?? false)
                {
                    parentDirectory?.Create();
                }
                
                await using var writer = new StreamWriter(new FileStream(path, FileMode.Create, FileAccess.Write));
                await writer.WriteAsync(file.Content);
            }

            // ビルド
            var (_, stdOut, stdErr) = ProcessX.GetDualAsyncEnumerable("cargo build --release --bin main", workingDirectory: tempDirectory);

            await Task.WhenAll(stdOut.WaitAsync(), stdErr.WaitAsync());

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

static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
{
    var dir = new DirectoryInfo(sourceDir);

    if (!dir.Exists)
        throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

    Directory.CreateDirectory(destinationDir);

    foreach (var file in dir.EnumerateFiles())
    {
        file.CopyTo(Path.Combine(destinationDir, file.Name));
    }

    if (recursive)
    {
        foreach (var subDir in dir.EnumerateDirectories())
        {
            var newDestinationDir = Path.Combine(destinationDir, subDir.Name);
            CopyDirectory(subDir.FullName, newDestinationDir, true);
        }
    }
}