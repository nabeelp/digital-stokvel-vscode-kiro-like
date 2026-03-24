using DigitalStokvel.NotificationService.Entities;

namespace DigitalStokvel.NotificationService.DTOs;

/// <summary>
/// Detailed notification information
/// </summary>
public class GetNotificationResponse
{
    /// <summary>
    /// Notification ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Recipient user ID
    /// </summary>
    public Guid RecipientId { get; set; }

    /// <summary>
    /// Delivery channel
    /// </summary>
    public NotificationChannel Channel { get; set; }

    /// <summary>
    /// Template identifier
    /// </summary>
    public string TemplateKey { get; set; } = string.Empty;

    /// <summary>
    /// Template variables
    /// </summary>
    public string Payload { get; set; } = "{}";

    /// <summary>
    /// Current status
    /// </summary>
    public NotificationStatus Status { get; set; }

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
    /// Error message if failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// When notification was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last update time
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
