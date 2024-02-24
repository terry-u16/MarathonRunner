using Cysharp.Diagnostics;
using Kurukuru;

namespace TerryU16.MarathonRunner.Infrastructures.GoogleCloud;

internal class IdTokenService
{
    private Task<string>? _idTokenTask;
    private DateTimeOffset _lastUpdatedTime;
    private readonly object _lockObject = new();
    private static readonly TimeSpan TokenExpiration = TimeSpan.FromMinutes(30);

    private static readonly IdTokenService _instance = new();

    public static IdTokenService Instance => _instance;

    public Task<string> GetIdTokenAsync(CancellationToken ct = default)
    {
        lock (_lockObject)
        {
            if (_idTokenTask is null || DateTimeOffset.Now - _lastUpdatedTime > TokenExpiration)
            {
                _idTokenTask = RequestTokenAsync(ct);
                _lastUpdatedTime = DateTimeOffset.Now;
            }
        }

        return _idTokenTask;
    }

    private static async Task<string> RequestTokenAsync(CancellationToken ct = default)
    {
        var token = "";

        await Spinner.StartAsync("Requesting token...", async spinner =>
        {
            try
            {
                token = await ProcessX.StartAsync("pwsh -c gcloud.ps1 auth print-identity-token").FirstAsync(ct);
                spinner.Succeed("Token acquired.");
            }
            catch (Exception)
            {
                spinner.Fail("Failed to get token.");
                throw;
            }
        });

        return token;
    }
}