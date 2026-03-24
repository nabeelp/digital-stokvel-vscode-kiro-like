using DigitalStokvel.Domain.Common;
using DigitalStokvel.Domain.Enums;

namespace DigitalStokvel.Domain.Entities;

/// <summary>
/// Represents a stokvel group
/// </summary>
public class Group : BaseEntity
{
    /// <summary>
    /// Group name
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Group description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Type of stokvel group
    /// </summary>
    public GroupType GroupType { get; set; }

    /// <summary>
    /// Fixed contribution amount per member
    /// </summary>
    public decimal ContributionAmount { get; set; }

    /// <summary>
    /// How often contributions are made
    /// </summary>
    public ContributionFrequency ContributionFrequency { get; set; }

    /// <summary>
    /// Payout schedule configuration (JSON)
    /// </summary>
    public string? PayoutSchedule { get; set; }

    /// <summary>
    /// Group constitution and rules (JSON)
    /// </summary>
    public string? Constitution { get; set; }

    /// <summary>
    /// Maximum number of members allowed
    /// </summary>
    public int MaxMembers { get; set; } = 50;

    /// <summary>
    /// Minimum quorum percentage for voting
    /// </summary>
    public decimal QuorumPercentage { get; set; } = 50.0m;

    /// <summary>
    /// Late payment penalty percentage
    /// </summary>
    public decimal LateFeePercentage { get; set; } = 5.0m;

    /// <summary>
    /// Current operational status
    /// </summary>
    public GroupStatus Status { get; set; } = GroupStatus.Active;

    /// <summary>
    /// Date when group was officially started
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Date when group ended (if dissolved)
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Group members
    /// </summary>
    public ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();

    /// <summary>
    /// Group savings account
    /// </summary>
    public GroupSavingsAccount? SavingsAccount { get; set; }

    /// <summary>
    /// Contributions to this group
    /// </summary>
    public ICollection<Contribution> Contributions { get; set; } = new List<Contribution>();

    /// <summary>
    /// Payouts from this group
    /// </summary>
    public ICollection<Payout> Payouts { get; set; } = new List<Payout>();

    /// <summary>
    /// Votes in this group
    /// </summary>
    public ICollection<Vote> Votes { get; set; } = new List<Vote>();

    /// <summary>
    /// Get current member count
    /// </summary>
    public int CurrentMemberCount => Members.Count(m => m.Status == MemberStatus.Active);

    /// <summary>
    /// Check if group is at capacity
    /// </summary>
    public bool IsAtCapacity => CurrentMemberCount >= MaxMembers;

    /// <summary>
    /// Check if group can accept new members
    /// </summary>
    public bool CanAcceptNewMembers => Status == GroupStatus.Active && !IsAtCapacity;
}
