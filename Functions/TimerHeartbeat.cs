using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FunctionsTraining.Functions;

// Timer trigger: runs every 5 minutes.
// NCRONTAB format: {second} {minute} {hour} {day} {month} {day-of-week}
public class TimerHeartbeat(ILogger<TimerHeartbeat> logger)
{
    [Function(nameof(TimerHeartbeat))]
    public void Run([TimerTrigger("0 */5 * * * *")] TimerInfo timer)
    {
        logger.LogInformation("Heartbeat at {Now:O} (UTC)", DateTime.UtcNow);

        if (timer.IsPastDue)
        {
            logger.LogWarning("Timer is running late (past due).");
        }

        if (timer.ScheduleStatus is not null)
        {
            logger.LogInformation("Next run at {Next:O}", timer.ScheduleStatus.Next);
        }
    }
}
