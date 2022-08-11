using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using TerryU16.MarathonRunner.Core;
using TerryU16.MarathonRunner.Core.Dispatchers;
using TerryU16.MarathonRunner.Core.Executors;

namespace TerryU16.MarathonRunner.Infrastructures.GoogleCloud;

public class GoogleCloudDispatcher : Dispatcher
{
    private readonly HttpClient _httpClient;
    private readonly Uri _endPoint;
    private readonly IdTokenService _tokenService;

    public GoogleCloudDispatcher(IOptions<ProblemOption> problemOptions, IOptions<ExecutionOption> executionOptions,
        IOptions<GoogleCloudOptions> googleCloudOption, ILogger<GoogleCloudDispatcher> logger, HttpClient httpClient) 
        : base(problemOptions, executionOptions, logger, executionOptions.Value.CloudExecutionSteps)
    {
        var endPoint = googleCloudOption.Value.RunEndpoint;
        ArgumentNullException.ThrowIfNull(endPoint);
        _endPoint = endPoint;
        _httpClient = httpClient;
        _tokenService = new IdTokenService();
    }

    protected override async Task<TestCaseResult> DispatchAsyncInner(SingleCaseExecutorArgs args, CancellationToken ct = default)
    {
        var contentJson = JsonConvert.SerializeObject(args);
        var request = new HttpRequestMessage(HttpMethod.Post, _endPoint);
        request.Content = new StringContent(contentJson, Encoding.UTF8, "application/json");
        request.Headers.Add("Authorization", $"Bearer {await _tokenService.GetIdTokenAsync(ct)}");

        var response = await _httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync(ct);
        var result = JsonConvert.DeserializeObject<TestCaseResult>(responseJson) ??
                     throw new FormatException("レスポンスのフォーマットが不正です。");
        return result;
    }
}