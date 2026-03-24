using DigitalStokvel.Domain.Common;
using DigitalStokvel.Domain.Enums;

namespace DigitalStokvel.Domain.Entities;

/// <summary>
/// Represents a payout from a group to its members
/// </summary>
public class Payout : BaseEntity
{
    /// <summary>
    /// Reference to the group
    /// </summary>
    public required Guid GroupId { get; set; }

    /// <summary>
    /// Type of payout
    /// </summary>
    public PayoutType PayoutType { get; set; }

    /// <summary>
    /// Total amount to be paid out
    /// </summary>
    public required decimal TotalAmount { get; set; }

    /// <summary>
    /// Current payout status
    /// </summary>
    public PayoutStatus Status { get; set; } = PayoutStatus.PendingApproval;

    /// <summary>
    /// User who initiated the payout
    /// </summary>
    public required Guid InitiatedBy { get; set; }

    /// <summary>
    /// User who approved the payout
    /// </summary>
    public Guid? ApprovedBy { get; set; }

    /// <summary>
    /// Timestamp when payout was initiated
    /// </summary>
    public DateTime InitiatedAt { get; set; }

    /// <summary>
    /// Timestamp when payout was approved
    /// </summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>
    /// Timestamp when payout was executed
    /// </summary>
    public DateTime? ExecutedAt { get; set; }

    /// <summary>
    /// Scheduled payout date
    /// </summary>
    public DateTime? ScheduledDate { get; set; }

    /// <summary>
    /// Reason for the payout
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Navigation property to Group
    /// </summary>
    public Group? Group { get; set; }

    /// <summary>
    /// Individual disbursements to members
    /// </summary>
    public ICollection<PayoutDisbursement> Disbursements { get; set; } = new List<PayoutDisbursement>();

    /// <summary>
    /// Check if payout requires approval
    /// </summary>
    public bool RequiresApproval => Status == PayoutStatus.PendingApproval;

    /// <summary>
    /// Check if dual approval is complete
    /// </summary>
    public bool IsDualApprovalComplete => InitiatedBy != default && ApprovedBy != default && InitiatedBy != ApprovedBy;
}
