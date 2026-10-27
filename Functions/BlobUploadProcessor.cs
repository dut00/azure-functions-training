using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FunctionsTraining.Functions;

// Blob trigger: runs when a file is uploaded to the "uploads" container.
// Flex Consumption supports only the Event Grid based blob trigger (no polling),
// so an Event Grid subscription on the storage account is required (see README).
public class BlobUploadProcessor(ILogger<BlobUploadProcessor> logger)
{
    [Function(nameof(BlobUploadProcessor))]
    [BlobOutput("processed/{name}.info.txt", Connection = "TrainingStorage")]
    public string Run(
        [BlobTrigger("uploads/{name}", Source = BlobTriggerSource.EventGrid, Connection = "TrainingStorage")] byte[] content,
        string name,
        FunctionContext context)
    {
        // Binding data contains blob properties such as the URI and metadata
        context.BindingContext.BindingData.TryGetValue("Uri", out var uri);

        logger.LogInformation("New blob '{Name}' ({Size} bytes) at {Uri}", name, content.Length, uri);

        // The returned string is written to processed/{name}.info.txt
        return $"""
            File: {name}
            Size: {content.Length} bytes
            Uri: {uri}
            Processed at (UTC): {DateTime.UtcNow:O}
            """;
    }
}
