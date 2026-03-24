namespace DigitalStokvel.UssdGateway.DTOs;

/// <summary>
/// DTO for group information from API Gateway
/// </summary>
public class GroupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string GroupType { get; set; } = string.Empty;
    public decimal ContributionAmount { get; set; }
    public string ContributionFrequency { get; set; } = string.Empty;
}

/// <summary>
/// DTO for user's groups list response
/// </summary>
public class UserGroupsResponseDto
{
    public List<GroupDto> Groups { get; set; } = new();
}

/// <summary>
/// DTO for contribution due amount response
/// </summary>
public class ContributionDueResponseDto
{
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
    public bool IsOverdue { get; set; }
}

/// <summary>
/// DTO for contribution payment request
/// </summary>
public class ContributionPaymentRequestDto
{
    public Guid GroupId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = "linked_account";
    public string Pin { get; set; } = string.Empty;
}

/// <summary>
/// DTO for contribution payment response
/// </summary>
public class ContributionPaymentResponseDto
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public decimal Amount { get; set; }
    public string TransactionRef { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public ReceiptDto? Receipt { get; set; }
    public DateTime? PaidAt { get; set; }
}

/// <summary>
/// DTO for contribution receipt
/// </summary>
public class ReceiptDto
{
    public string ReceiptNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
}

/// <summary>
/// DTO for group balance response
/// </summary>
public class GroupBalanceResponseDto
{
    public Guid GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public decimal TotalContributions { get; set; }
    public decimal TotalPayouts { get; set; }
}
