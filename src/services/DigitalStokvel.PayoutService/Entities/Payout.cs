using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalStokvel.PayoutService.Entities;

/// <summary>
/// Represents a payout request in a stokvel group.
/// Requires dual approval workflow: initiated by Chairperson, approved by Treasurer.
/// </summary>
[Table("payouts")]
public class Payout
{
    /// <summary>
    /// Unique identifier for the payout.
    /// </summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// ID of the group this payout belongs to.
    /// </summary>
    [Required]
    [Column("group_id")]
    public Guid GroupId { get; set; }

    /// <summary>
    /// Type of payout (rotating, year_end_pot, emergency).
    /// </summary>
    [Required]
    [Column("payout_type")]
    public PayoutType PayoutType { get; set; }

    /// <summary>
    /// Total amount to be paid out (must be positive).
    /// </summary>
    [Required]
    [Column("total_amount")]
    [Range(0.01, 9999999999.99)]
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Current status of the payout.
    /// </summary>
    [Required]
    [Column("status")]
    public PayoutStatus Status { get; set; } = PayoutStatus.PendingApproval;

    /// <summary>
    /// ID of the group member who initiated the payout (typically Chairperson).
    /// </summary>
    [Required]
    [Column("initiated_by")]
    public Guid InitiatedBy { get; set; }

    /// <summary>
    /// ID of the group member who approved the payout (typically Treasurer).
    /// Null until approved.
    /// </summary>
    [Column("approved_by")]
    public Guid? ApprovedBy { get; set; }

    /// <summary>
    /// Timestamp when the payout was initiated.
    /// </summary>
    [Required]
    [Column("initiated_at")]
    public DateTime InitiatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the payout was approved by Treasurer.
    /// Null until approved.
    /// </summary>
    [Column("approved_at")]
    public DateTime? ApprovedAt { get; set; }

    /// <summary>
    /// Timestamp when the payout was executed (funds disbursed).
    /// Null until executed.
    /// </summary>
    [Column("executed_at")]
    public DateTime? ExecutedAt { get; set; }

    /// <summary>
    /// Timestamp when the payout record was created.
    /// </summary>
    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the payout record was last updated.
    /// </summary>
    [Required]
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property for disbursements associated with this payout.
    /// </summary>
    public ICollection<PayoutDisbursement> Disbursements { get; set; } = new List<PayoutDisbursement>();
}
