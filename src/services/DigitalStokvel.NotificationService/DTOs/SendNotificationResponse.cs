using DigitalStokvel.NotificationService.Entities;

namespace DigitalStokvel.NotificationService.DTOs;

/// <summary>
/// Response after sending a notification
/// </summary>
public class SendNotificationResponse
{
    /// <summary>
    /// Notification ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Current notification status
    /// </summary>
    public NotificationStatus Status { get; set; }

    /// <summary>
    /// Message for client
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// When the notification was created
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
