using System.ComponentModel;

namespace DigitalStokvel.PayoutService.Entities;

/// <summary>
/// Type of payout for a stokvel group.
/// </summary>
public enum PayoutType
{
    /// <summary>
    /// Rotating payout where members take turns receiving the pot.
    /// </summary>
    [Description("rotating")]
    Rotating,

    /// <summary>
    /// Year-end pot distribution where all members receive their share.
    /// </summary>
    [Description("year_end_pot")]
    YearEndPot,

    /// <summary>
    /// Emergency payout triggered by exceptional circumstances.
    /// </summary>
    [Description("emergency")]
    Emergency
}

/// <summary>
/// Status of a payout request.
/// </summary>
public enum PayoutStatus
{
    /// <summary>
    /// Payout initiated by Chairperson, awaiting Treasurer approval.
    /// </summary>
    [Description("pending_approval")]
    PendingApproval,

    /// <summary>
    /// Payout approved by Treasurer, ready for execution.
    /// </summary>
    [Description("approved")]
    Approved,

    /// <summary>
    /// Payout has been executed and funds disbursed.
    /// </summary>
    [Description("executed")]
    Executed,

    /// <summary>
    /// Payout execution failed due to insufficient funds or other errors.
    /// </summary>
    [Description("failed")]
    Failed
}
