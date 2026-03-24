namespace DigitalStokvel.GroupService.Entities;

/// <summary>
/// Type of stokvel group
/// </summary>
public enum GroupType
{
    /// <summary>
    /// Rotating payout where members receive payouts in turns
    /// </summary>
    RotatingPayout,
    
    /// <summary>
    /// Savings pot with year-end distribution
    /// </summary>
    SavingsPot,
    
    /// <summary>
    /// Investment club for collective investments
    /// </summary>
    InvestmentClub
}

/// <summary>
/// Frequency of contributions
/// </summary>
public enum ContributionFrequency
{
    /// <summary>
    /// Weekly contributions
    /// </summary>
    Weekly,
    
    /// <summary>
    /// Bi-weekly contributions
    /// </summary>
    BiWeekly,
    
    /// <summary>
    /// Monthly contributions
    /// </summary>
    Monthly
}

/// <summary>
/// Status of a group
/// </summary>
public enum GroupStatus
{
    /// <summary>
    /// Group is active and accepting contributions
    /// </summary>
    Active,
    
    /// <summary>
    /// Group is suspended (no new contributions)
    /// </summary>
    Suspended,
    
    /// <summary>
    /// Group has been closed
    /// </summary>
    Closed
}

/// <summary>
/// Member role in a group
/// </summary>
public enum MemberRole
{
    /// <summary>
    /// Regular member (default)
    /// </summary>
    Member,
    
    /// <summary>
    /// Secretary - manages communications and records
    /// </summary>
    Secretary,
    
    /// <summary>
    /// Treasurer - manages finances and approves payouts
    /// </summary>
    Treasurer,
    
    /// <summary>
    /// Chairperson - leads the group and has full permissions
    /// </summary>
    Chairperson
}

/// <summary>
/// Member status in a group
/// </summary>
public enum MemberStatus
{
    /// <summary>
    /// Active member who can contribute
    /// </summary>
    Active,
    
    /// <summary>
    /// Suspended member (cannot contribute)
    /// </summary>
    Suspended,
    
    /// <summary>
    /// Member has left the group
    /// </summary>
    Left,
    
    /// <summary>
    /// Pending invitation acceptance
    /// </summary>
    Pending
}
