using System.ComponentModel.DataAnnotations;

namespace DigitalStokvel.PayoutService.DTOs;

// ============================================================================
// REQUEST DTOs
// ============================================================================

/// <summary>
/// Request to initiate a payout for a group (typically by Chairperson).
/// </summary>
public class InitiatePayoutRequest
{
    /// <summary>
    /// ID of the group for which the payout is being initiated.
    /// </summary>
    [Required]
    public Guid GroupId { get; set; }

    /// <summary>
    /// Type of payout: "rotating", "year_end_pot", or "emergency".
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string PayoutType { get; set; } = string.Empty;

    /// <summary>
    /// ID of the member receiving the payout (for rotating payouts).
    /// Optional for year-end pot (all members receive a share).
    /// </summary>
    public Guid? RecipientMemberId { get; set; }

    /// <summary>
    /// Total amount to be paid out.
    /// </summary>
    [Required]
    [Range(0.01, 9999999999.99)]
    public decimal Amount { get; set; }

    /// <summary>
    /// Optional note explaining the payout.
    /// </summary>
    [MaxLength(500)]
    public string? Note { get; set; }
}

/// <summary>
/// Request to approve a payout (typically by Treasurer).
/// </summary>
public class ApprovePayoutRequest
{
    /// <summary>
    /// Encrypted PIN for authorization.
    /// </summary>
    [Required]
    public string Pin { get; set; } = string.Empty;

    /// <summary>
    /// Optional comment from the approver.
    /// </summary>
    [MaxLength(500)]
    public string? Comment { get; set; }
}

// ============================================================================
// RESPONSE DTOs
// ============================================================================

/// <summary>
/// Response after successfully initiating a payout.
/// </summary>
public class InitiatePayoutResponse
{
    /// <summary>
    /// Unique ID of the created payout.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID of the group.
    /// </summary>
    public Guid GroupId { get; set; }

    /// <summary>
    /// Type of payout.
    /// </summary>
    public string PayoutType { get; set; } = string.Empty;

    /// <summary>
    /// Total amount to be paid out.
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Current status of the payout.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Information about who initiated the payout.
    /// </summary>
    public InitiatorDto InitiatedBy { get; set; } = new();

    /// <summary>
    /// Timestamp when the payout was initiated.
    /// </summary>
    public DateTime InitiatedAt { get; set; }

    /// <summary>
    /// Role required to approve this payout (typically "treasurer").
    /// </summary>
    public string RequiresApprovalFrom { get; set; } = string.Empty;
}

/// <summary>
/// Response after successfully approving a payout.
/// </summary>
public class ApprovePayoutResponse
{
    /// <summary>
    /// Unique ID of the payout.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Updated status of the payout.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Information about who approved the payout.
    /// </summary>
    public ApproverDto ApprovedBy { get; set; } = new();

    /// <summary>
    /// Timestamp when the payout was approved.
    /// </summary>
    public DateTime ApprovedAt { get; set; }

    /// <summary>
    /// Execution status (e.g., "processing", "completed", "failed").
    /// </summary>
    public string ExecutionStatus { get; set; } = string.Empty;
}

/// <summary>
/// Response containing detailed payout status and disbursement information.
/// </summary>
public class PayoutStatusResponse
{
    /// <summary>
    /// Unique ID of the payout.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID of the group.
    /// </summary>
    public Guid GroupId { get; set; }

    /// <summary>
    /// Type of payout.
    /// </summary>
    public string PayoutType { get; set; } = string.Empty;

    /// <summary>
    /// Total amount to be paid out.
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Current status of the payout.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// List of disbursements for this payout.
    /// </summary>
    public List<DisbursementDto> Disbursements { get; set; } = new();

    /// <summary>
    /// Timestamp when the payout was initiated.
    /// </summary>
    public DateTime InitiatedAt { get; set; }

    /// <summary>
    /// Timestamp when the payout was approved (null if not yet approved).
    /// </summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>
    /// Timestamp when the payout was executed (null if not yet executed).
    /// </summary>
    public DateTime? ExecutedAt { get; set; }

    /// <summary>
    /// Information about who initiated the payout.
    /// </summary>
    public InitiatorDto InitiatedBy { get; set; } = new();

    /// <summary>
    /// Information about who approved the payout (null if not yet approved).
    /// </summary>
    public ApproverDto? ApprovedBy { get; set; }
}

// ============================================================================
// SUPPORTING DTOs
// ============================================================================

/// <summary>
/// Information about an individual disbursement within a payout.
/// </summary>
public class DisbursementDto
{
    /// <summary>
    /// Member receiving this disbursement.
    /// </summary>
    public MemberDto Member { get; set; } = new();

    /// <summary>
    /// Amount disbursed to this member.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Unique transaction reference from payment gateway or bank.
    /// </summary>
    public string? TransactionRef { get; set; }

    /// <summary>
    /// Status of this disbursement.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the disbursement was executed (null if not yet executed).
    /// </summary>
    public DateTime? ExecutedAt { get; set; }
}

/// <summary>
/// Information about a member (used in various contexts).
/// </summary>
public class MemberDto
{
    /// <summary>
    /// Unique ID of the member.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Name of the member.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Phone number of the member.
    /// </summary>
    public string Phone { get; set; } = string.Empty;
}

/// <summary>
/// Information about the initiator of a payout.
/// </summary>
public class InitiatorDto
{
    /// <summary>
    /// Unique ID of the initiator.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Name of the initiator.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Role of the initiator in the group (e.g., "chairperson").
    /// </summary>
    public string Role { get; set; } = string.Empty;
}

/// <summary>
/// Information about the approver of a payout.
/// </summary>
public class ApproverDto
{
    /// <summary>
    /// Unique ID of the approver.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Name of the approver.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Role of the approver in the group (e.g., "treasurer").
    /// </summary>
    public string Role { get; set; } = string.Empty;
}

/// <summary>
/// Response containing payout history for a group.
/// </summary>
public class GroupPayoutHistoryResponse
{
    /// <summary>
    /// ID of the group.
    /// </summary>
    public Guid GroupId { get; set; }

    /// <summary>
    /// List of payouts for the group.
    /// </summary>
    public List<PayoutSummaryDto> Payouts { get; set; } = new();

    /// <summary>
    /// Total number of payouts for the group.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Number of payouts skipped (for pagination).
    /// </summary>
    public int Skip { get; set; }

    /// <summary>
    /// Maximum number of payouts returned (for pagination).
    /// </summary>
    public int Take { get; set; }
}

/// <summary>
/// Summary information about a payout (for list views).
/// </summary>
public class PayoutSummaryDto
{
    /// <summary>
    /// Unique ID of the payout.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Type of payout.
    /// </summary>
    public string PayoutType { get; set; } = string.Empty;

    /// <summary>
    /// Total amount paid out.
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Current status of the payout.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Number of disbursements in this payout.
    /// </summary>
    public int DisbursementCount { get; set; }

    /// <summary>
    /// Timestamp when the payout was initiated.
    /// </summary>
    public DateTime InitiatedAt { get; set; }

    /// <summary>
    /// Timestamp when the payout was approved (null if not yet approved).
    /// </summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>
    /// Timestamp when the payout was executed (null if not yet executed).
    /// </summary>
    public DateTime? ExecutedAt { get; set; }
}
