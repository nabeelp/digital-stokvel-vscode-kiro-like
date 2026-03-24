using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalStokvel.UssdGateway.Entities;

/// <summary>
/// Represents a USSD session for tracking user interactions
/// Sessions have a 120-second timeout as per design requirements
/// </summary>
[Table("ussd_sessions")]
public class UssdSession
{
    /// <summary>
    /// Unique session identifier from USSD aggregator
    /// </summary>
    [Key]
    [MaxLength(100)]
    [Column("session_id")]
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// User's phone number in E.164 format (+27821234567)
    /// </summary>
    [Required]
    [MaxLength(20)]
    [Column("phone_number")]
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// User ID if authenticated, null for unauthenticated sessions
    /// </summary>
    [Column("user_id")]
    public Guid? UserId { get; set; }

    /// <summary>
    /// Current menu level (1-3, design constraint: max 3 levels)
    /// </summary>
    [Column("menu_level")]
    public int MenuLevel { get; set; } = 1;

    /// <summary>
    /// Preferred language code (en, zu, xh, st, af)
    /// </summary>
    [Required]
    [MaxLength(5)]
    [Column("language")]
    public string Language { get; set; } = "en";

    /// <summary>
    /// Serialized JSON context for session state
    /// Stores current action, selected group, payment details, etc.
    /// </summary>
    [Required]
    [Column("context", TypeName = "jsonb")]
    public string Context { get; set; } = "{}";

    /// <summary>
    /// Last USSD string entered by user
    /// </summary>
    [MaxLength(200)]
    [Column("last_input")]
    public string? LastInput { get; set; }

    /// <summary>
    /// Session creation timestamp
    /// </summary>
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last activity timestamp for timeout tracking (120 seconds)
    /// </summary>
    [Column("last_activity_at")]
    public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Session expiration timestamp (CreatedAt + 120 seconds)
    /// </summary>
    [Column("expires_at")]
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Whether the session has been terminated
    /// </summary>
    [Column("is_terminated")]
    public bool IsTerminated { get; set; } = false;
}
