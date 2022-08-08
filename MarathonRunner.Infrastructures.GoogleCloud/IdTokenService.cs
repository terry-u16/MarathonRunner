using Cysharp.Diagnostics;
using Kurukuru;

namespace TerryU16.MarathonRunner.Infrastructures.GoogleCloud;

internal class IdTokenService
{
    private Task<string>? _idTokenTask;
    private DateTimeOffset _lastUpdatedTime = DateTimeOffset.Now;
    private readonly object _lockObject = new();
    private static readonly TimeSpan TokenExpiration = TimeSpan.FromMinutes(30);

    public Task<string> GetIdTokenAsync()
    {
        lock (_lockObject)
        {
            if (_idTokenTask is null || DateTimeOffset.Now - _lastUpdatedTime > TokenExpiration)
            {
                _idTokenTask = RequestTokenAsync();
                _lastUpdatedTime = DateTimeOffset.Now;
            }
        }

        return _idTokenTask;
    }

    private static async Task<string> RequestTokenAsync()
    {
        var token = "";

        await Spinner.StartAsync("Requesting token...", async spinner =>
        {
            try
            {
                token = await ProcessX.StartAsync("pwsh -c gcloud.ps1 auth print-identity-token").FirstAsync();
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