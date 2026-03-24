using System.ComponentModel;

namespace DigitalStokvel.GovernanceService.Entities;

/// <summary>
/// Status of a vote within a group.
/// </summary>
public enum VoteStatus
{
    /// <summary>
    /// Vote is in draft state, not yet active.
    /// </summary>
    [Description("draft")]
    Draft,

    /// <summary>
    /// Vote is active and members can cast their votes.
    /// </summary>
    [Description("active")]
    Active,

    /// <summary>
    /// Vote has ended and results are final.
    /// </summary>
    [Description("closed")]
    Closed
}

/// <summary>
/// Type of dispute raised by a member.
/// </summary>
public enum DisputeType
{
    /// <summary>
    /// Dispute related to missed or late payment issues.
    /// </summary>
    [Description("missed_payment")]
    MissedPayment,

    /// <summary>
    /// Dispute alleging fraudulent activity.
    /// </summary>
    [Description("fraud")]
    Fraud,

    /// <summary>
    /// Dispute related to violation of group constitution.
    /// </summary>
    [Description("constitution_violation")]
    ConstitutionViolation,

    /// <summary>
    /// Other type of dispute not covered by specific categories.
    /// </summary>
    [Description("other")]
    Other
}

/// <summary>
/// Status of a dispute resolution process.
/// </summary>
public enum DisputeStatus
{
    /// <summary>
    /// Dispute has been raised and is awaiting review.
    /// </summary>
    [Description("open")]
    Open,

    /// <summary>
    /// Dispute is under investigation by group leadership.
    /// </summary>
    [Description("investigating")]
    Investigating,

    /// <summary>
    /// Dispute has been resolved.
    /// </summary>
    [Description("resolved")]
    Resolved,

    /// <summary>
    /// Dispute has been escalated to higher authority or external mediation.
    /// </summary>
    [Description("escalated")]
    Escalated
}
