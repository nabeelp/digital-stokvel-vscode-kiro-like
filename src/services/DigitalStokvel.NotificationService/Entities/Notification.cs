using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalStokvel.NotificationService.Entities;

/// <summary>
/// Represents a multi-channel notification with delivery tracking
/// Maps to notifications table in database (V006 migration)
/// </summary>
[Table("notifications")]
public class Notification
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("recipient_id")]
    public Guid RecipientId { get; set; }

    [Required]
    [Column("channel")]
    [MaxLength(10)]
    public NotificationChannel Channel { get; set; }

    [Required]
    [Column("template_key")]
    [MaxLength(100)]
    public string TemplateKey { get; set; } = string.Empty;

    /// <summary>
    /// Template variables as JSON object (e.g., {"memberName": "John", "amount": "100.00"})
    /// </summary>
    [Required]
    [Column("payload", TypeName = "jsonb")]
    public string Payload { get; set; } = "{}";

    [Required]
    [Column("status")]
    [MaxLength(20)]
    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;

    [Column("sent_at")]
    public DateTime? SentAt { get; set; }

    [Column("delivered_at")]
    public DateTime? DeliveredAt { get; set; }

    [Required]
    [Column("retry_count")]
    [Range(0, 3)]
    public int RetryCount { get; set; } = 0;

    [Column("error_message")]
    public string? ErrorMessage { get; set; }

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
