using DigitalStokvel.Domain.Common;

namespace DigitalStokvel.Domain.Entities;

/// <summary>
/// Immutable ledger entry for contributions (append-only audit trail)
/// </summary>
public class ContributionLedger : AuditEntity
{
    /// <summary>
    /// Reference to the contribution
    /// </summary>
    public required Guid ContributionId { get; init; }

    /// <summary>
    /// Reference to the group
    /// </summary>
    public required Guid GroupId { get; init; }

    /// <summary>
    /// Reference to the member
    /// </summary>
    public required Guid MemberId { get; init; }

    /// <summary>
    /// Amount recorded in the ledger
    /// </summary>
    public required decimal Amount { get; init; }

    /// <summary>
    /// Transaction reference from payment gateway
    /// </summary>
    public required string TransactionReference { get; init; }

    /// <summary>
    /// Additional metadata (JSON)
    /// </summary>
    public string? Metadata { get; init; }

    /// <summary>
    /// Balance after this transaction
    /// </summary>
    public decimal BalanceAfter { get; init; }

    /// <summary>
    /// Navigation property to Contribution
    /// </summary>
    public Contribution? Contribution { get; init; }
}
