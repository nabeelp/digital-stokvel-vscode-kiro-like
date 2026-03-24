namespace DigitalStokvel.NotificationService.Services;

/// <summary>
/// USSD notification provider for feature phone support
/// Simulated implementation for development
/// TODO: Integrate with actual USSD aggregator in production
/// </summary>
public class UssdNotificationProvider : INotificationProvider
{
    private readonly ILogger<UssdNotificationProvider> _logger;

    public UssdNotificationProvider(ILogger<UssdNotificationProvider> logger)
    {
        _logger = logger;
    }

    public string Channel => "Ussd";

    public async Task<bool> SendAsync(Guid recipientId, string message)
    {
        try
        {
            _logger.LogInformation(
                "Sending USSD message to user {RecipientId}: {Message}",
                recipientId,
                message);

            // Simulate network delay
            await Task.Delay(150);

            // Simulate success (85% success rate in dev)
            var success = Random.Shared.Next(100) < 85;

            if (success)
            {
                _logger.LogInformation(
                    "USSD message sent successfully to user {RecipientId}",
                    recipientId);
            }
            else
            {
                _logger.LogWarning(
                    "USSD message delivery failed for user {RecipientId}",
                    recipientId);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error sending USSD message to user {RecipientId}",
                recipientId);
            return false;
        }
    }
}
