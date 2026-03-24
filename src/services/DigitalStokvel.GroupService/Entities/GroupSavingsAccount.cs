using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalStokvel.GroupService.Entities;

/// <summary>
/// Group savings account entity
/// </summary>
[Table("group_savings_accounts")]
public class GroupSavingsAccount
{
    /// <summary>
    /// Unique identifier for the account
    /// </summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    /// <summary>
    /// Group ID (one-to-one relationship)
    /// </summary>
    [Required]
    [Column("group_id")]
    public Guid GroupId { get; set; }

    /// <summary>
    /// Bank account number from core banking system
    /// </summary>
    [Required]
    [Column("account_number")]
    [MaxLength(20)]
    public string AccountNumber { get; set; } = string.Empty;

    /// <summary>
    /// Current account balance in ZAR
    /// </summary>
    [Required]
    [Column("balance", TypeName = "decimal(12, 2)")]
    [Range(0, double.MaxValue)]
    public decimal Balance { get; set; }

    /// <summary>
    /// Total contributions received
    /// </summary>
    [Required]
    [Column("total_contributions", TypeName = "decimal(12, 2)")]
    [Range(0, double.MaxValue)]
    public decimal TotalContributions { get; set; }

    /// <summary>
    /// Total interest earned
    /// </summary>
    [Required]
    [Column("total_interest_earned", TypeName = "decimal(12, 2)")]
    [Range(0, double.MaxValue)]
    public decimal TotalInterestEarned { get; set; }

    /// <summary>
    /// Total payouts made
    /// </summary>
    [Required]
    [Column("total_payouts", TypeName = "decimal(12, 2)")]
    [Range(0, double.MaxValue)]
    public decimal TotalPayouts { get; set; }

    /// <summary>
    /// Annual interest rate percentage (0-20%)
    /// </summary>
    [Required]
    [Column("interest_rate", TypeName = "decimal(5, 2)")]
    [Range(0, 20)]
    public decimal InterestRate { get; set; }

    /// <summary>
    /// Timestamp of last interest calculation
    /// </summary>
    [Column("last_interest_calculation_at")]
    public DateTime? LastInterestCalculationAt { get; set; }

    /// <summary>
    /// Timestamp when the account was created
    /// </summary>
    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the account was last updated
    /// </summary>
    [Required]
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    
    /// <summary>
    /// The group this account belongs to
    /// </summary>
    [ForeignKey(nameof(GroupId))]
    public virtual Group? Group { get; set; }
}
