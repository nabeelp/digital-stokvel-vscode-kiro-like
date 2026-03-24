namespace DigitalStokvel.NotificationService.Services;

/// <summary>
/// Interface for notification delivery providers (Push, SMS, USSD)
/// </summary>
public interface INotificationProvider
{
    /// <summary>
    /// Sends a notification using the provider's channel
    /// </summary>
    /// <param name="recipientId">User ID to receive notification</param>
    /// <param name="message">Formatted message content</param>
    /// <returns>True if sent successfully, false otherwise</returns>
    Task<bool> SendAsync(Guid recipientId, string message);

    /// <summary>
    /// Gets the channel this provider handles
    /// </summary>
    string Channel { get; }
}
