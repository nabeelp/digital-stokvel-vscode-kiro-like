using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalStokvel.PayoutService.Entities;

/// <summary>
/// Represents an individual member disbursement for an approved payout.
/// Each payout can have multiple disbursements (e.g., for year-end pot distribution).
/// </summary>
[Table("payout_disbursements")]
public class PayoutDisbursement
{
    /// <summary>
    /// Unique identifier for the disbursement.
    /// </summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// ID of the payout this disbursement belongs to.
    /// </summary>
    [Required]
    [Column("payout_id")]
    public Guid PayoutId { get; set; }

    /// <summary>
    /// ID of the group member receiving this disbursement.
    /// </summary>
    [Required]
    [Column("member_id")]
    public Guid MemberId { get; set; }

    /// <summary>
    /// Amount to be disbursed to the member (must be positive).
    /// </summary>
    [Required]
    [Column("amount")]
    [Range(0.01, 9999999999.99)]
    public decimal Amount { get; set; }

    /// <summary>
    /// Unique transaction reference from payment gateway or bank.
    /// Populated when disbursement is executed.
    /// </summary>
    [Column("transaction_ref")]
    [MaxLength(100)]
    public string? TransactionRef { get; set; }

    /// <summary>
    /// Status of the disbursement (same enum as Payout).
    /// </summary>
    [Required]
    [Column("status")]
    public PayoutStatus Status { get; set; } = PayoutStatus.PendingApproval;

    /// <summary>
    /// Timestamp when the disbursement was executed.
    /// Null until executed.
    /// </summary>
    [Column("executed_at")]
    public DateTime? ExecutedAt { get; set; }

    /// <summary>
    /// Timestamp when the disbursement record was created.
    /// </summary>
    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the disbursement record was last updated.
    /// </summary>
    [Required]
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property to the parent payout.
    /// </summary>
    public Payout? Payout { get; set; }
}
