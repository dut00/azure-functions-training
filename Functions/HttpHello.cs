using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FunctionsTraining.Functions;

// HTTP trigger: GET /api/hello?name=Kuba  or  POST /api/hello  { "name": "Kuba" }
public class HttpHello(ILogger<HttpHello> logger)
{
    [Function(nameof(HttpHello))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "hello")] HttpRequest req)
    {
        logger.LogInformation("HTTP {Method} request received", req.Method);

        string? name = req.Query["name"];

        // Fall back to the JSON body for POST requests
        if (string.IsNullOrWhiteSpace(name) && HttpMethods.IsPost(req.Method))
        {
            try
            {
                var body = await JsonSerializer.DeserializeAsync<HelloRequest>(
                    req.Body, new JsonSerializerOptions(JsonSerializerDefaults.Web));
                name = body?.Name;
            }
            catch (JsonException)
            {
                return new BadRequestObjectResult(new { error = "Invalid JSON body." });
            }
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return new BadRequestObjectResult(new { error = "Pass 'name' in the query string or JSON body." });
        }

        return new OkObjectResult(new
        {
            message = $"Hello, {name}! This is Azure Functions.",
            timestampUtc = DateTime.UtcNow
        });
    }

    private record HelloRequest(string? Name);
}
