using DigitalStokvel.Domain.Common;
using DigitalStokvel.Domain.Enums;

namespace DigitalStokvel.Domain.Entities;

/// <summary>
/// Represents an individual disbursement as part of a payout
/// </summary>
public class PayoutDisbursement : BaseEntity
{
    /// <summary>
    /// Reference to the parent payout
    /// </summary>
    public required Guid PayoutId { get; set; }

    /// <summary>
    /// Reference to the member receiving the disbursement
    /// </summary>
    public required Guid MemberId { get; set; }

    /// <summary>
    /// Amount being disbursed to this member
    /// </summary>
    public required decimal Amount { get; set; }

    /// <summary>
    /// Transaction reference from payment system
    /// </summary>
    public string? TransactionReference { get; set; }

    /// <summary>
    /// Status of this specific disbursement
    /// </summary>
    public ContributionStatus Status { get; set; } = ContributionStatus.Pending;

    /// <summary>
    /// Timestamp when disbursement was completed
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Error message if disbursement failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Navigation property to Payout
    /// </summary>
    public Payout? Payout { get; set; }

    /// <summary>
    /// Navigation property to GroupMember
    /// </summary>
    public GroupMember? Member { get; set; }
}
