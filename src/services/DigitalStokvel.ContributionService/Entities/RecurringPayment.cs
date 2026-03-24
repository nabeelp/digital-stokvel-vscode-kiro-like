using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalStokvel.ContributionService.Entities;

/// <summary>
/// Recurring payment entity mapped to recurring_payments table
/// </summary>
[Table("recurring_payments")]
public class RecurringPayment
{
    /// <summary>
    /// Unique identifier for the recurring payment
    /// </summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Group member ID who set up the recurring payment
    /// </summary>
    [Required]
    [Column("member_id")]
    public Guid MemberId { get; set; }

    /// <summary>
    /// Group ID this recurring payment belongs to
    /// </summary>
    [Required]
    [Column("group_id")]
    public Guid GroupId { get; set; }

    /// <summary>
    /// Recurring payment amount (must be positive)
    /// </summary>
    [Required]
    [Range(0.01, 999999.99)]
    [Column("amount", TypeName = "decimal(10,2)")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Payment frequency (weekly, biweekly, monthly)
    /// </summary>
    [Required]
    [Column("frequency")]
    public ContributionFrequency Frequency { get; set; }

    /// <summary>
    /// Recurring payment status (active, paused, cancelled)
    /// </summary>
    [Required]
    [MaxLength(20)]
    [Column("status")]
    public RecurringPaymentStatus Status { get; set; } = RecurringPaymentStatus.Active;

    /// <summary>
    /// Date of the next scheduled payment
    /// </summary>
    [Required]
    [Column("next_payment_date")]
    public DateTime NextPaymentDate { get; set; }

    /// <summary>
    /// Timestamp when recurring payment was created
    /// </summary>
    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when recurring payment was last updated
    /// </summary>
    [Required]
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when recurring payment was paused (nullable if active)
    /// </summary>
    [Column("paused_at")]
    public DateTime? PausedAt { get; set; }

    /// <summary>
    /// Timestamp when recurring payment was cancelled (nullable if active)
    /// </summary>
    [Column("cancelled_at")]
    public DateTime? CancelledAt { get; set; }
}
