using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalStokvel.GroupService.Entities;

/// <summary>
/// Stokvel group entity
/// </summary>
[Table("groups")]
public class Group
{
    /// <summary>
    /// Unique identifier for the group
    /// </summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    /// <summary>
    /// Group name (max 50 characters)
    /// </summary>
    [Required]
    [Column("name")]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Group description (max 200 characters, optional)
    /// </summary>
    [Column("description")]
    [MaxLength(200)]
    public string? Description { get; set; }

    /// <summary>
    /// Type of stokvel group
    /// </summary>
    [Required]
    [Column("group_type")]
    public GroupType GroupType { get; set; }

    /// <summary>
    /// Fixed contribution amount per period (R50-R10,000)
    /// </summary>
    [Required]
    [Column("contribution_amount", TypeName = "decimal(10, 2)")]
    [Range(50, 10000)]
    public decimal ContributionAmount { get; set; }

    /// <summary>
    /// Frequency of contributions
    /// </summary>
    [Required]
    [Column("contribution_frequency")]
    public ContributionFrequency ContributionFrequency { get; set; }

    /// <summary>
    /// Payout schedule configuration (JSON)
    /// </summary>
    [Required]
    [Column("payout_schedule", TypeName = "jsonb")]
    public string PayoutSchedule { get; set; } = "{}";

    /// <summary>
    /// Group governance rules (grace period, late fees, quorum, etc.) (JSON)
    /// </summary>
    [Required]
    [Column("constitution", TypeName = "jsonb")]
    public string Constitution { get; set; } = "{\"grace_period_days\":3,\"late_fee\":50.00,\"missed_payments_threshold\":3,\"quorum_percentage\":51}";

    /// <summary>
    /// Current status of the group
    /// </summary>
    [Required]
    [Column("status")]
    public GroupStatus Status { get; set; } = GroupStatus.Active;

    /// <summary>
    /// User ID who created the group (Chairperson)
    /// </summary>
    [Required]
    [Column("created_by")]
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Timestamp when the group was created
    /// </summary>
    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the group was last updated
    /// </summary>
    [Required]
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the group was closed (null if still active)
    /// </summary>
    [Column("closed_at")]
    public DateTime? ClosedAt { get; set; }

    // Navigation properties
    
    /// <summary>
    /// Members of this group
    /// </summary>
    public virtual ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();

    /// <summary>
    /// Savings account for this group
    /// </summary>
    public virtual GroupSavingsAccount? SavingsAccount { get; set; }
}
