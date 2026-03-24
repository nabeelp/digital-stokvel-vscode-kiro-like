using DigitalStokvel.NotificationService.Entities;

namespace DigitalStokvel.NotificationService.DTOs;

/// <summary>
/// Summary of notification delivery status
/// </summary>
public class NotificationStatusResponse
{
    /// <summary>
    /// Notification ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Current delivery status
    /// </summary>
    public NotificationStatus Status { get; set; }

    /// <summary>
    /// Delivery channel used
    /// </summary>
    public NotificationChannel Channel { get; set; }

    /// <summary>
    /// When notification was sent
    /// </summary>
    public DateTime? SentAt { get; set; }

    /// <summary>
    /// When notification was delivered
    /// </summary>
    public DateTime? DeliveredAt { get; set; }

    /// <summary>
    /// Number of retry attempts
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// Error message if delivery failed
    /// </summary>
    public string? ErrorMessage { get; set; }
}
