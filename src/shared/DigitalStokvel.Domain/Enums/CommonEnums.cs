namespace DigitalStokvel.Domain.Enums;

/// <summary>
/// Contribution status lifecycle
/// </summary>
public enum ContributionStatus
{
    Pending = 0,
    Completed = 1,
    Failed = 2,
    Cancelled = 3
}

/// <summary>
/// Payout status workflow
/// </summary>
public enum PayoutStatus
{
    PendingApproval = 0,
    Approved = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4,
    Cancelled = 5
}

/// <summary>
/// Dispute resolution status
/// </summary>
public enum DisputeStatus
{
    Open = 0,
    UnderReview = 1,
    Resolved = 2,
    Closed = 3
}

/// <summary>
/// Notification delivery channels
/// </summary>
public enum NotificationChannel
{
    Push = 0,
    Sms = 1,
    Ussd = 2,
    Email = 3
}

/// <summary>
/// Notification delivery status
/// </summary>
public enum NotificationStatus
{
    Pending = 0,
    Sent = 1,
    Delivered = 2,
    Failed = 3
}

/// <summary>
/// Group member roles based on stokvel governance
/// </summary>
public enum GroupRole
{
    Member = 0,
    Chairperson = 1,
    Treasurer = 2,
    Secretary = 3
}

/// <summary>
/// Group member status
/// </summary>
public enum MemberStatus
{
    Active = 0,
    Suspended = 1,
    Left = 2,
    Expelled = 3
}

/// <summary>
/// Group operational status
/// </summary>
public enum GroupStatus
{
    Active = 0,
    Inactive = 1,
    Suspended = 2,
    Dissolved = 3
}

/// <summary>
/// Type of stokvel group
/// </summary>
public enum GroupType
{
    Rotating = 0,      // Rotating savings and credit association (ROSCA)
    Burial = 1,        // Burial society
    Investment = 2,    // Investment club
    Grocery = 3,       // Grocery stokvel
    Christmas = 4      // Christmas/savings club
}

/// <summary>
/// Contribution frequency
/// </summary>
public enum ContributionFrequency
{
    Weekly = 0,
    Biweekly = 1,
    Monthly = 2,
    Quarterly = 3
}

/// <summary>
/// Payout type
/// </summary>
public enum PayoutType
{
    Rotating = 0,           // Regular rotating payout to members
    YearEnd = 1,           // Year-end pot distribution
    Emergency = 2,         // Emergency payout (e.g., funeral)
    Disbursement = 3       // General disbursement
}

/// <summary>
/// Vote type for governance decisions
/// </summary>
public enum VoteType
{
    Constitutional = 0,    // Changes to group constitution
    Membership = 1,        // New member admission
    Emergency = 2,         // Emergency decisions
    General = 3           // General governance matters
}

/// <summary>
/// Vote response
/// </summary>
public enum VoteResponse
{
    Yes = 0,
    No = 1,
    Abstain = 2
}

/// <summary>
/// Preferred language for the platform
/// </summary>
public enum Language
{
    English = 0,
    Zulu = 1,
    Xhosa = 2,
    Afrikaans = 3,
    Sesotho = 4
}
