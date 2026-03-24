using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalStokvel.GovernanceService.Entities;

/// <summary>
/// Represents a dispute raised by a member for resolution by group governance.
/// </summary>
[Table("disputes")]
public class Dispute
{
    /// <summary>
    /// Unique identifier for the dispute.
    /// </summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// ID of the group this dispute belongs to.
    /// </summary>
    [Required]
    [Column("group_id")]
    public Guid GroupId { get; set; }

    /// <summary>
    /// ID of the group member who raised the dispute.
    /// </summary>
    [Required]
    [Column("raised_by")]
    public Guid RaisedBy { get; set; }

    /// <summary>
    /// Type of dispute (missed_payment, fraud, constitution_violation, other).
    /// </summary>
    [Required]
    [Column("dispute_type")]
    public DisputeType DisputeType { get; set; }

    /// <summary>
    /// Detailed description of the dispute.
    /// </summary>
    [Required]
    [Column("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Evidence supporting the dispute, stored as JSON array.
    /// Example: [{"type": "image", "url": "..."}, {"type": "document", "url": "..."}]
    /// </summary>
    [Column("evidence", TypeName = "jsonb")]
    public string Evidence { get; set; } = "[]";

    /// <summary>
    /// Current status of the dispute (open, investigating, resolved, escalated).
    /// </summary>
    [Required]
    [Column("status")]
    public DisputeStatus Status { get; set; } = DisputeStatus.Open;

    /// <summary>
    /// Notes explaining the resolution.
    /// Required when status is 'resolved'.
    /// </summary>
    [Column("resolution_notes")]
    public string? ResolutionNotes { get; set; }

    /// <summary>
    /// Timestamp when the dispute was raised.
    /// </summary>
    [Required]
    [Column("raised_at")]
    public DateTime RaisedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the dispute was resolved.
    /// Required when status is 'resolved'.
    /// </summary>
    [Column("resolved_at")]
    public DateTime? ResolvedAt { get; set; }

    /// <summary>
    /// Timestamp when the dispute record was created.
    /// </summary>
    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the dispute record was last updated.
    /// </summary>
    [Required]
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
