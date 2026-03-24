using System.ComponentModel.DataAnnotations;
using DigitalStokvel.NotificationService.Entities;

namespace DigitalStokvel.NotificationService.DTOs;

/// <summary>
/// Request to send a notification
/// </summary>
public class SendNotificationRequest
{
    /// <summary>
    /// User ID to receive the notification
    /// </summary>
    [Required]
    public Guid RecipientId { get; set; }

    /// <summary>
    /// Delivery channel (Push, Sms, Ussd)
    /// </summary>
    [Required]
    public NotificationChannel Channel { get; set; }

    /// <summary>
    /// Template identifier for message content
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string TemplateKey { get; set; } = string.Empty;

    /// <summary>
    /// Template variables (e.g., {"memberName": "John", "amount": "100.00"})
    /// </summary>
    public Dictionary<string, string> Payload { get; set; } = new();

    /// <summary>
    /// Preferred language code (en, zu, st, xh, af)
    /// </summary>
    [MaxLength(2)]
    public string? LanguageCode { get; set; } = "en";
}
