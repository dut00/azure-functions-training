using Azure.Messaging.EventGrid;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FunctionsTraining.Functions;

// Event Grid trigger: receives events from an Event Grid subscription,
// e.g. Microsoft.Storage.BlobDeleted from the training storage account.
public class EventGridLogger(ILogger<EventGridLogger> logger)
{
    [Function(nameof(EventGridLogger))]
    public void Run([EventGridTrigger] EventGridEvent eventGridEvent)
    {
        logger.LogInformation(
            "Event received. Type: {Type}, Subject: {Subject}, Time: {Time}",
            eventGridEvent.EventType,
            eventGridEvent.Subject,
            eventGridEvent.EventTime);

        logger.LogInformation("Event data: {Data}", eventGridEvent.Data.ToString());

        if (eventGridEvent.EventType == "Microsoft.Storage.BlobDeleted")
        {
            logger.LogWarning("A blob was deleted: {Subject}", eventGridEvent.Subject);
        }
    }
}
