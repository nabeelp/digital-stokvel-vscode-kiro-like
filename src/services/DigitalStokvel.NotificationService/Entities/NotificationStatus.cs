namespace DigitalStokvel.NotificationService.Entities;

/// <summary>
/// Lifecycle status of a notification
/// </summary>
public enum NotificationStatus
{
    /// <summary>
    /// Notification queued for delivery
    /// </summary>
    Pending,

    /// <summary>
    /// Notification sent to provider
    /// </summary>
    Sent,

    /// <summary>
    /// Notification successfully delivered
    /// </summary>
    Delivered,

    /// <summary>
    /// Notification delivery failed
    /// </summary>
    Failed
}
