using System.Text;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using TerryU16.MarathonRunner.Core;
using TerryU16.MarathonRunner.Core.Compilers;

namespace TerryU16.MarathonRunner.Infrastructures.GoogleCloud.Compilers;

public class RustCompileDispatcher : ICompileDispatcher
{
    private readonly string _problemName;
    private readonly string _bucketName;
    private readonly string _exeName;
    private readonly Uri _endPoint;
    private readonly CompileFile[] _compileFiles;
    private readonly HttpClient _httpClient;
    private readonly IdTokenService _tokenService;

    public RustCompileDispatcher(IOptions<ProblemOption> problemOption, IOptions<CompileOption> compileOption, 
        IOptions<GoogleCloudOptions> googleCloudOption, HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(googleCloudOption.Value.RustCompilerEndpoint);
        _endPoint = googleCloudOption.Value.RustCompilerEndpoint;
        _problemName = problemOption.Value.ProblemName;
        _bucketName = googleCloudOption.Value.BucketName;
        _exeName = compileOption.Value.ExeName;
        _compileFiles = compileOption.Value.Files;
        _httpClient = httpClient;
        _tokenService = new IdTokenService();
    }

    public async Task CompileAsync(CancellationToken ct = default)
    {
        var compileContents = new List<CompileContent>();

        foreach (var compileFile in _compileFiles)
        {
            var content = await File.ReadAllTextAsync(compileFile.Source, ct);
            compileContents.Add(new CompileContent(compileFile.Destination, content));
        }

        var args = new RustCompileArgs(_bucketName, _problemName, _exeName, compileContents.ToArray());
        var contentJson = JsonConvert.SerializeObject(args);
        var request = new HttpRequestMessage(HttpMethod.Post, _endPoint);
        request.Content = new StringContent(contentJson, Encoding.UTF8, "application/json");
        request.Headers.Add("Authorization", $"Bearer {await _tokenService.GetIdTokenAsync(ct)}");

        var response = await _httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
    }
}