using DigitalStokvel.Domain.Common;
using DigitalStokvel.Domain.Enums;

namespace DigitalStokvel.Domain.Entities;

/// <summary>
/// Represents a member's association with a stokvel group
/// </summary>
public class GroupMember : BaseEntity
{
    /// <summary>
    /// Reference to the group
    /// </summary>
    public required Guid GroupId { get; set; }

    /// <summary>
    /// Reference to the user
    /// </summary>
    public required Guid UserId { get; set; }

    /// <summary>
    /// Member's role in the group
    /// </summary>
    public GroupRole Role { get; set; } = GroupRole.Member;

    /// <summary>
    /// Current membership status
    /// </summary>
    public MemberStatus Status { get; set; } = MemberStatus.Active;

    /// <summary>
    /// Date when member joined
    /// </summary>
    public DateTime JoinedAt { get; set; }

    /// <summary>
    /// Date when member left (if applicable)
    /// </summary>
    public DateTime? LeftAt { get; set; }

    /// <summary>
    /// Total contributions made by this member
    /// </summary>
    public decimal TotalContributions { get; set; }

    /// <summary>
    /// Number of missed payments
    /// </summary>
    public int MissedPaymentCount { get; set; }

    /// <summary>
    /// Navigation property to Group
    /// </summary>
    public Group? Group { get; set; }

    /// <summary>
    /// Navigation property to User
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// Contributions made by this member
    /// </summary>
    public ICollection<Contribution> Contributions { get; set; } = new List<Contribution>();

    /// <summary>
    /// Check if member has leadership role
    /// </summary>
    public bool IsLeader => Role is GroupRole.Chairperson or GroupRole.Treasurer or GroupRole.Secretary;

    /// <summary>
    /// Check if member can approve payouts
    /// </summary>
    public bool CanApprovePayouts => Role is GroupRole.Chairperson or GroupRole.Treasurer;
}
