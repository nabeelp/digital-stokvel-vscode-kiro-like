using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalStokvel.ContributionService.Entities;

/// <summary>
/// Contribution entity mapped to contributions table
/// </summary>
[Table("contributions")]
public class Contribution
{
    /// <summary>
    /// Unique identifier for the contribution
    /// </summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Group ID this contribution belongs to
    /// </summary>
    [Required]
    [Column("group_id")]
    public Guid GroupId { get; set; }

    /// <summary>
    /// Group member ID who made the contribution
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
    /// Contribution status (pending, paid, failed, refunded)
    /// </summary>
    [Required]
    [Column("status")]
    public ContributionStatus Status { get; set; } = ContributionStatus.Pending;

    /// <summary>
    /// Unique transaction reference from payment gateway
    /// </summary>
    [MaxLength(100)]
    [Column("transaction_ref")]
    public string? TransactionRef { get; set; }

    /// <summary>
    /// Date when contribution is due
    /// </summary>
    [Required]
    [Column("due_date")]
    public DateTime DueDate { get; set; }

    /// <summary>
    /// Timestamp when payment was confirmed (nullable until paid)
    /// </summary>
    [Column("paid_at")]
    public DateTime? PaidAt { get; set; }

    /// <summary>
    /// Timestamp when contribution was created
    /// </summary>
    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when contribution was last updated
    /// </summary>
    [Required]
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
