namespace DigitalStokvel.NotificationService.Services;

/// <summary>
/// SMS notification provider (Twilio, Clickatell, Vodacom)
/// Simulated implementation for development
/// TODO: Integrate with actual SMS gateway in production
/// </summary>
public class SmsNotificationProvider : INotificationProvider
{
    private readonly ILogger<SmsNotificationProvider> _logger;

    public SmsNotificationProvider(ILogger<SmsNotificationProvider> logger)
    {
        _logger = logger;
    }

    public string Channel => "Sms";

    public async Task<bool> SendAsync(Guid recipientId, string message)
    {
        try
        {
            _logger.LogInformation(
                "Sending SMS to user {RecipientId}: {Message}",
                recipientId,
                message);

            // Simulate network delay
            await Task.Delay(200);

            // Simulate success (90% success rate in dev)
            var success = Random.Shared.Next(100) < 90;

            if (success)
            {
                _logger.LogInformation(
                    "SMS sent successfully to user {RecipientId}",
                    recipientId);
            }
            else
            {
                _logger.LogWarning(
                    "SMS delivery failed for user {RecipientId}",
                    recipientId);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error sending SMS to user {RecipientId}",
                recipientId);
            return false;
        }
    }
}
