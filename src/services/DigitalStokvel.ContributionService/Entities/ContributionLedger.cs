using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalStokvel.ContributionService.Entities;

/// <summary>
/// Contribution ledger entity - IMMUTABLE append-only audit trail
/// Mapped to contribution_ledger table
/// </summary>
[Table("contribution_ledger")]
public class ContributionLedger
{
    /// <summary>
    /// Unique identifier for the ledger entry
    /// </summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Reference to the contribution record
    /// </summary>
    [Required]
    [Column("contribution_id")]
    public Guid ContributionId { get; set; }

    /// <summary>
    /// Group ID for this contribution
    /// </summary>
    [Required]
    [Column("group_id")]
    public Guid GroupId { get; set; }

    /// <summary>
    /// Member ID who made the contribution
    /// </summary>
    [Required]
    [Column("member_id")]
    public Guid MemberId { get; set; }

    /// <summary>
    /// Contribution amount (must be positive)
    /// </summary>
    [Required]
    [Range(0.01, 999999.99)]
    [Column("amount", TypeName = "decimal(10,2)")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Unique transaction reference from payment gateway
    /// </summary>
    [Required]
    [MaxLength(100)]
    [Column("transaction_ref")]
    public string TransactionRef { get; set; } = string.Empty;

    /// <summary>
    /// Additional metadata stored as JSON (payment method, gateway response, etc.)
    /// </summary>
    [Column("metadata", TypeName = "jsonb")]
    public string Metadata { get; set; } = "{}";

    /// <summary>
    /// Immutable timestamp when ledger entry was recorded
    /// </summary>
    [Required]
    [Column("recorded_at")]
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    /// <summary>
    /// Navigation property to the contribution record
    /// </summary>
    public Contribution? Contribution { get; set; }
}
