namespace DigitalStokvel.NotificationService.Entities;

/// <summary>
/// Delivery channel for notifications
/// </summary>
public enum NotificationChannel
{
    /// <summary>
    /// Push notification to mobile device
    /// </summary>
    Push,

    /// <summary>
    /// SMS text message
    /// </summary>
    Sms,

    /// <summary>
    /// USSD session message
    /// </summary>
    Ussd
}
