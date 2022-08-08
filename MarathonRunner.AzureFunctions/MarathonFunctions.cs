using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using TerryU16.MarathonRunner.Core;
using TerryU16.MarathonRunner.Core.Executors;

namespace MarathonRunner.AzureFunctions
{
    public class MarathonFunctions
    {
        private readonly ILogger<MarathonFunctions> _logger;

        public MarathonFunctions(ILogger<MarathonFunctions> log)
        {
            _logger = log;
        }

        [FunctionName("Run")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiRequestBody("application/json", typeof(SingleCaseExecutorArgs), Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(TestCaseResult))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var args = await JsonSerializer.DeserializeAsync<SingleCaseExecutorArgs>(req.Body);

            if (args is null)
            {
                return new BadRequestResult();
            }

            var timeLimit = args.Timeout / 2;
            _logger.LogError(timeLimit.ToString());

            var sw = Stopwatch.StartNew();
            long counter = 0;

            while (true)
            {
                if ((counter++ & ((1 << 20) - 1)) == 0 && sw.Elapsed > timeLimit)
                {
                    break;
                }
            }

            var result = new TestCaseResult(counter, sw.Elapsed);
            return new OkObjectResult(result);
        }
    }
}

