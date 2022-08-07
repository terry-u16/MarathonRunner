using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using TerryU16.MarathonRunner.Core.Executors;

namespace TerryU16.MarathonRunner.Core.Dispatchers;

public class CloudDispatcher : Dispatcher
{
    private readonly HttpClient _httpClient;
    private readonly Uri _endPoint;

    public CloudDispatcher(IOptions<ProblemOption> problemOptions, IOptions<ExecutionOption> executionOptions, ILogger<CloudDispatcher> logger,
        HttpClient httpClient) : base(problemOptions, executionOptions, logger)
    {
        var endPoint = executionOptions.Value.CloudEndpoint;
        ArgumentNullException.ThrowIfNull(endPoint);
        _endPoint = endPoint;
        _httpClient = httpClient;
    }

    private protected override async Task<TestCaseResult> DispatchAsyncInner(SingleCaseExecutorArgs args, CancellationToken ct = default)
    {
        var contentJson = JsonConvert.SerializeObject(args);
        var content = new StringContent(contentJson, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(_endPoint, content, ct);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync(ct);
        var result = JsonConvert.DeserializeObject<TestCaseResult>(responseJson) ??
                     throw new FormatException("レスポンスのフォーマットが不正です。");
        return result;
    }
}