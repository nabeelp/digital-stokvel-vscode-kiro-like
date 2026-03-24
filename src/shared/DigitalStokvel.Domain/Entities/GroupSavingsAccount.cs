using DigitalStokvel.Domain.Common;

namespace DigitalStokvel.Domain.Entities;

/// <summary>
/// Represents the group's savings account at the bank
/// </summary>
public class GroupSavingsAccount : BaseEntity
{
    /// <summary>
    /// Reference to the group
    /// </summary>
    public required Guid GroupId { get; set; }

    /// <summary>
    /// Bank account number
    /// </summary>
    public required string AccountNumber { get; set; }

    /// <summary>
    /// Current balance
    /// </summary>
    public decimal Balance { get; set; }

    /// <summary>
    /// Total of all contributions received
    /// </summary>
    public decimal TotalContributions { get; set; }

    /// <summary>
    /// Total interest earned
    /// </summary>
    public decimal TotalInterestEarned { get; set; }

    /// <summary>
    /// Total payouts disbursed
    /// </summary>
    public decimal TotalPayouts { get; set; }

    /// <summary>
    /// Last interest capitalization date
    /// </summary>
    public DateTime? LastInterestDate { get; set; }

    /// <summary>
    /// Navigation property to Group
    /// </summary>
    public Group? Group { get; set; }

    /// <summary>
    /// Calculate available balance for payouts
    /// </summary>
    public decimal AvailableBalance => Balance - TotalPayouts;
}
