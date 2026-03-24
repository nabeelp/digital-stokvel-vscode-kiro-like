using System.ComponentModel.DataAnnotations;

namespace DigitalStokvel.ContributionService.Entities;

/// <summary>
/// Contribution status enum matching database contribution_status type
/// </summary>
public enum ContributionStatus
{
    /// <summary>
    /// Contribution is pending payment
    /// </summary>
    Pending,

    /// <summary>
    /// Contribution has been paid
    /// </summary>
    Paid,

    /// <summary>
    /// Contribution payment failed
    /// </summary>
    Failed,

    /// <summary>
    /// Contribution was refunded
    /// </summary>
    Refunded
}

/// <summary>
/// Contribution frequency enum matching database contribution_frequency type
/// </summary>
public enum ContributionFrequency
{
    /// <summary>
    /// Weekly contributions
    /// </summary>
    Weekly,

    /// <summary>
    /// Bi-weekly contributions (every 2 weeks)
    /// </summary>
    BiWeekly,

    /// <summary>
    /// Monthly contributions
    /// </summary>
    Monthly
}

/// <summary>
/// Recurring payment status
/// </summary>
public enum RecurringPaymentStatus
{
    /// <summary>
    /// Recurring payment is active
    /// </summary>
    Active,

    /// <summary>
    /// Recurring payment is paused
    /// </summary>
    Paused,

    /// <summary>
    /// Recurring payment has been cancelled
    /// </summary>
    Cancelled
}
