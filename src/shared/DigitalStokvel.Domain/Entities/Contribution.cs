using DigitalStokvel.Domain.Common;
using DigitalStokvel.Domain.Enums;

namespace DigitalStokvel.Domain.Entities;

/// <summary>
/// Represents a contribution made by a member to their group
/// </summary>
public class Contribution : BaseEntity
{
    /// <summary>
    /// Reference to the group
    /// </summary>
    public required Guid GroupId { get; set; }

    /// <summary>
    /// Reference to the member making the contribution
    /// </summary>
    public required Guid MemberId { get; set; }

    /// <summary>
    /// Contribution amount
    /// </summary>
    public required decimal Amount { get; set; }

    /// <summary>
    /// Current status of the contribution
    /// </summary>
    public ContributionStatus Status { get; set; } = ContributionStatus.Pending;

    /// <summary>
    /// External payment transaction reference
    /// </summary>
    public string? TransactionReference { get; set; }

    /// <summary>
    /// Due date for this contribution
    /// </summary>
    public DateTime DueDate { get; set; }

    /// <summary>
    /// Date when payment was completed
    /// </summary>
    public DateTime? PaidAt { get; set; }

    /// <summary>
    /// Late fee applied (if payment was late)
    /// </summary>
    public decimal LateFee { get; set; }

    /// <summary>
    /// Notes or comments about the contribution
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Navigation property to Group
    /// </summary>
    public Group? Group { get; set; }

    /// <summary>
    /// Navigation property to GroupMember
    /// </summary>
    public GroupMember? Member { get; set; }

    /// <summary>
    /// Corresponding ledger entry
    /// </summary>
    public ContributionLedger? LedgerEntry { get; set; }

    /// <summary>
    /// Check if contribution is overdue
    /// </summary>
    public bool IsOverdue => Status == ContributionStatus.Pending && DueDate < DateTime.UtcNow;

    /// <summary>
    /// Get total amount including late fees
    /// </summary>
    public decimal TotalAmount => Amount + LateFee;
}
