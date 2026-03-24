namespace DigitalStokvel.ContributionService.DTOs;

/// <summary>
/// Request to create a new contribution
/// </summary>
public class CreateContributionRequest
{
    /// <summary>
    /// Group ID to contribute to
    /// </summary>
    public required Guid GroupId { get; set; }

    /// <summary>
    /// Contribution amount in ZAR
    /// </summary>
    public required decimal Amount { get; set; }

    /// <summary>
    /// Payment method (linked_account, debit_order, eft)
    /// </summary>
    public required string PaymentMethod { get; set; }

    /// <summary>
    /// Encrypted PIN for payment authorization
    /// </summary>
    public required string Pin { get; set; }
}

/// <summary>
/// Response after creating a contribution
/// </summary>
public class CreateContributionResponse
{
    /// <summary>
    /// Contribution ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Group ID
    /// </summary>
    public Guid GroupId { get; set; }

    /// <summary>
    /// Contribution amount
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Unique transaction reference
    /// </summary>
    public string TransactionRef { get; set; } = string.Empty;

    /// <summary>
    /// Contribution status
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Payment receipt details
    /// </summary>
    public ReceiptDto? Receipt { get; set; }

    /// <summary>
    /// Timestamp when payment was processed
    /// </summary>
    public DateTime? PaidAt { get; set; }
}

/// <summary>
/// Payment receipt details
/// </summary>
public class ReceiptDto
{
    /// <summary>
    /// Unique receipt number
    /// </summary>
    public string ReceiptNumber { get; set; } = string.Empty;

    /// <summary>
    /// Receipt date
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Group name
    /// </summary>
    public string GroupName { get; set; } = string.Empty;

    /// <summary>
    /// Contribution amount
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Group account balance after contribution
    /// </summary>
    public decimal BalanceAfter { get; set; }

    /// <summary>
    /// URL to download PDF receipt
    /// </summary>
    public string? PdfUrl { get; set; }
}

/// <summary>
/// Response for contribution history listing
/// </summary>
public class ContributionHistoryResponse
{
    /// <summary>
    /// List of contributions
    /// </summary>
    public List<ContributionDto> Contributions { get; set; } = new();

    /// <summary>
    /// Summary statistics
    /// </summary>
    public ContributionSummaryDto? Summary { get; set; }

    /// <summary>
    /// Pagination information
    /// </summary>
    public PaginationDto? Pagination { get; set; }
}

/// <summary>
/// Individual contribution details
/// </summary>
public class ContributionDto
{
    /// <summary>
    /// Contribution ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Member who made the contribution
    /// </summary>
    public MemberDto? Member { get; set; }

    /// <summary>
    /// Contribution amount
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Transaction reference
    /// </summary>
    public string TransactionRef { get; set; } = string.Empty;

    /// <summary>
    /// Contribution status
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Due date for the contribution
    /// </summary>
    public DateTime DueDate { get; set; }

    /// <summary>
    /// Timestamp when payment was made
    /// </summary>
    public DateTime? PaidAt { get; set; }
}

/// <summary>
/// Member information in contribution
/// </summary>
public class MemberDto
{
    /// <summary>
    /// Member ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Member name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Member phone number
    /// </summary>
    public string Phone { get; set; } = string.Empty;
}

/// <summary>
/// Contribution summary statistics
/// </summary>
public class ContributionSummaryDto
{
    /// <summary>
    /// Total contributions amount
    /// </summary>
    public decimal TotalContributions { get; set; }

    /// <summary>
    /// Number of members who have paid
    /// </summary>
    public int TotalMembersPaid { get; set; }

    /// <summary>
    /// Number of members with pending contributions
    /// </summary>
    public int TotalMembersPending { get; set; }

    /// <summary>
    /// Number of members with overdue contributions
    /// </summary>
    public int TotalMembersOverdue { get; set; }
}

/// <summary>
/// Pagination metadata
/// </summary>
public class PaginationDto
{
    /// <summary>
    /// Current page number
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int Limit { get; set; }

    /// <summary>
    /// Total number of items
    /// </summary>
    public int TotalItems { get; set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages { get; set; }
}

/// <summary>
/// Request to set up a recurring payment
/// </summary>
public class CreateRecurringPaymentRequest
{
    /// <summary>
    /// Group ID
    /// </summary>
    public required Guid GroupId { get; set; }

    /// <summary>
    /// Recurring payment amount
    /// </summary>
    public required decimal Amount { get; set; }

    /// <summary>
    /// Payment frequency (monthly, weekly, biweekly)
    /// </summary>
    public required string Frequency { get; set; }

    /// <summary>
    /// Start date for recurring payments
    /// </summary>
    public required DateTime StartDate { get; set; }

    /// <summary>
    /// Payment method
    /// </summary>
    public required string PaymentMethod { get; set; }
}

/// <summary>
/// Response after creating recurring payment
/// </summary>
public class CreateRecurringPaymentResponse
{
    /// <summary>
    /// Recurring payment ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Group ID
    /// </summary>
    public Guid GroupId { get; set; }

    /// <summary>
    /// Recurring payment amount
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Payment frequency
    /// </summary>
    public string Frequency { get; set; } = string.Empty;

    /// <summary>
    /// Status (active, paused, cancelled)
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Date of next payment
    /// </summary>
    public DateTime NextPaymentDate { get; set; }

    /// <summary>
    /// Timestamp when recurring payment was created
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Response for getting user's recurring payments
/// </summary>
public class RecurringPaymentsResponse
{
    /// <summary>
    /// List of active recurring payments
    /// </summary>
    public List<RecurringPaymentDto> RecurringPayments { get; set; } = new();

    /// <summary>
    /// Total count
    /// </summary>
    public int TotalCount { get; set; }
}

/// <summary>
/// Individual recurring payment details
/// </summary>
public class RecurringPaymentDto
{
    /// <summary>
    /// Recurring payment ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Group name
    /// </summary>
    public string GroupName { get; set; } = string.Empty;

    /// <summary>
    /// Recurring payment amount
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Payment frequency
    /// </summary>
    public string Frequency { get; set; } = string.Empty;

    /// <summary>
    /// Status
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Next payment date
    /// </summary>
    public DateTime NextPaymentDate { get; set; }
}
