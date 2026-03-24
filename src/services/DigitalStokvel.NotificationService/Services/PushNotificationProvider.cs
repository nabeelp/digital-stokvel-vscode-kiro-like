namespace DigitalStokvel.NotificationService.Services;

/// <summary>
/// Push notification provider (Firebase Cloud Messaging, APNs)
/// Simulated implementation for development
/// TODO: Integrate with actual Firebase/APNs in production
/// </summary>
public class PushNotificationProvider : INotificationProvider
{
    private readonly ILogger<PushNotificationProvider> _logger;

    public PushNotificationProvider(ILogger<PushNotificationProvider> logger)
    {
        _logger = logger;
    }

    public string Channel => "Push";

    public async Task<bool> SendAsync(Guid recipientId, string message)
    {
        try
        {
            _logger.LogInformation(
                "Sending push notification to user {RecipientId}: {Message}",
                recipientId,
                message);

            // Simulate network delay
            await Task.Delay(100);

            // Simulate success (95% success rate in dev)
            var success = Random.Shared.Next(100) < 95;

            if (success)
            {
                _logger.LogInformation(
                    "Push notification sent successfully to user {RecipientId}",
                    recipientId);
            }
            else
            {
                _logger.LogWarning(
                    "Push notification failed for user {RecipientId}",
                    recipientId);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error sending push notification to user {RecipientId}",
                recipientId);
            return false;
        }
    }
}
