using DigitalStokvel.GroupService.Entities;

namespace DigitalStokvel.GroupService.DTOs;

/// <summary>
/// Request to create a new group
/// </summary>
public class CreateGroupRequest
{
    /// <summary>
    /// Group name (max 50 characters)
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Group description (max 200 characters, optional)
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Type of stokvel group
    /// </summary>
    public required string GroupType { get; set; }

    /// <summary>
    /// Fixed contribution amount per period (R50-R10,000)
    /// </summary>
    public decimal ContributionAmount { get; set; }

    /// <summary>
    /// Frequency of contributions
    /// </summary>
    public required string ContributionFrequency { get; set; }

    /// <summary>
    /// Payout schedule configuration
    /// </summary>
    public required PayoutScheduleDto PayoutSchedule { get; set; }

    /// <summary>
    /// Group governance rules
    /// </summary>
    public ConstitutionDto? Constitution { get; set; }

    /// <summary>
    /// Members to invite
    /// </summary>
    public List<InviteMemberDto>? InvitedMembers { get; set; }
}

/// <summary>
/// Payout schedule configuration
/// </summary>
public class PayoutScheduleDto
{
    /// <summary>
    /// Type of payout (year_end, rotating, monthly)
    /// </summary>
    public required string Type { get; set; }

    /// <summary>
    /// Payout date (for year_end and scheduled types)
    /// </summary>
    public DateTime? Date { get; set; }

    /// <summary>
    /// Payout day of month (for rotating monthly)
    /// </summary>
    public int? DayOfMonth { get; set; }
}

/// <summary>
/// Group constitution/governance rules
/// </summary>
public class ConstitutionDto
{
    /// <summary>
    /// Grace period in days for late payments (default: 3)
    /// </summary>
    public int GracePeriodDays { get; set; } = 3;

    /// <summary>
    /// Late fee amount in ZAR (default: R50)
    /// </summary>
    public decimal LateFee { get; set; } = 50.00m;

    /// <summary>
    /// Threshold for missed payments before suspension (default: 3)
    /// </summary>
    public int MissedPaymentsThreshold { get; set; } = 3;

    /// <summary>
    /// Quorum percentage for voting (default: 51%)
    /// </summary>
    public int QuorumPercentage { get; set; } = 51;
}

/// <summary>
/// Member invitation details
/// </summary>
public class InviteMemberDto
{
    /// <summary>
    /// Phone number in E.164 format
    /// </summary>
    public required string PhoneNumber { get; set; }

    /// <summary>
    /// Role to assign (member, secretary, treasurer, chairperson)
    /// </summary>
    public string Role { get; set; } = "member";
}

/// <summary>
/// Response after creating a group
/// </summary>
public class CreateGroupResponse
{
    /// <summary>
    /// Group ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Group name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Group type
    /// </summary>
    public string GroupType { get; set; } = string.Empty;

    /// <summary>
    /// Contribution amount
    /// </summary>
    public decimal ContributionAmount { get; set; }

    /// <summary>
    /// Contribution frequency
    /// </summary>
    public string ContributionFrequency { get; set; } = string.Empty;

    /// <summary>
    /// Group status
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Bank account number
    /// </summary>
    public string? AccountNumber { get; set; }

    /// <summary>
    /// Current member count
    /// </summary>
    public int MemberCount { get; set; }

    /// <summary>
    /// Timestamp when created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Invitation links for invited members
    /// </summary>
    public List<InvitationLinkDto>? InvitationLinks { get; set; }
}

/// <summary>
/// Invitation link details
/// </summary>
public class InvitationLinkDto
{
    /// <summary>
    /// Member ID
    /// </summary>
    public Guid MemberId { get; set; }

    /// <summary>
    /// Invitation token
    /// </summary>
    public string InviteToken { get; set; } = string.Empty;

    /// <summary>
    /// Shareable link
    /// </summary>
    public string ShareLink { get; set; } = string.Empty;
}

/// <summary>
/// Group details response
/// </summary>
public class GroupDetailsResponse
{
    /// <summary>
    /// Group ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Group name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Group description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Group type
    /// </summary>
    public string GroupType { get; set; } = string.Empty;

    /// <summary>
    /// Contribution amount
    /// </summary>
    public decimal ContributionAmount { get; set; }

    /// <summary>
    /// Contribution frequency
    /// </summary>
    public string ContributionFrequency { get; set; } = string.Empty;

    /// <summary>
    /// Group status
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Account details
    /// </summary>
    public AccountDetailsDto? Account { get; set; }

    /// <summary>
    /// Current member count
    /// </summary>
    public int MemberCount { get; set; }

    /// <summary>
    /// Next contribution due date
    /// </summary>
    public DateTime? NextContributionDue { get; set; }

    /// <summary>
    /// Next payout details
    /// </summary>
    public NextPayoutDto? NextPayout { get; set; }

    /// <summary>
    /// Group constitution
    /// </summary>
    public ConstitutionDto? Constitution { get; set; }

    /// <summary>
    /// Timestamp when created
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Account details
/// </summary>
public class AccountDetailsDto
{
    /// <summary>
    /// Bank account number
    /// </summary>
    public string AccountNumber { get; set; } = string.Empty;

    /// <summary>
    /// Current balance
    /// </summary>
    public decimal Balance { get; set; }

    /// <summary>
    /// Total contributions
    /// </summary>
    public decimal TotalContributions { get; set; }

    /// <summary>
    /// Total interest earned
    /// </summary>
    public decimal TotalInterestEarned { get; set; }

    /// <summary>
    /// Interest rate percentage
    /// </summary>
    public decimal InterestRate { get; set; }
}

/// <summary>
/// Next payout details
/// </summary>
public class NextPayoutDto
{
    /// <summary>
    /// Payout type
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Payout date
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Estimated payout amount
    /// </summary>
    public decimal EstimatedAmount { get; set; }
}

/// <summary>
/// User's groups list response
/// </summary>
public class UserGroupsResponse
{
    /// <summary>
    /// List of groups
    /// </summary>
    public List<GroupSummaryDto> Groups { get; set; } = new();

    /// <summary>
    /// Total number of groups
    /// </summary>
    public int TotalCount { get; set; }
}

/// <summary>
/// Group summary for list views
/// </summary>
public class GroupSummaryDto
{
    /// <summary>
    /// Group ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Group name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Group type
    /// </summary>
    public string GroupType { get; set; } = string.Empty;

    /// <summary>
    /// Contribution amount
    /// </summary>
    public decimal ContributionAmount { get; set; }

    /// <summary>
    /// Member count
    /// </summary>
    public int MemberCount { get; set; }

    /// <summary>
    /// User's role in this group
    /// </summary>
    public string UserRole { get; set; } = string.Empty;

    /// <summary>
    /// Current balance
    /// </summary>
    public decimal Balance { get; set; }
}
