using DigitalStokvel.PayoutService.DTOs;

namespace DigitalStokvel.PayoutService.Services;

/// <summary>
/// Service interface for payout operations.
/// </summary>
public interface IPayoutService
{
    /// <summary>
    /// Initiates a new payout for a group (typically by Chairperson).
    /// Creates a payout request with status "pending_approval".
    /// </summary>
    Task<InitiatePayoutResponse> InitiatePayoutAsync(InitiatePayoutRequest request, Guid userId);

    /// <summary>
    /// Approves a payout (typically by Treasurer).
    /// Updates status to "approved" and triggers execution.
    /// </summary>
    Task<ApprovePayoutResponse> ApprovePayoutAsync(Guid payoutId, ApprovePayoutRequest request, Guid userId);

    /// <summary>
    /// Retrieves the status of a payout including disbursements.
    /// </summary>
    Task<PayoutStatusResponse> GetPayoutStatusAsync(Guid payoutId);
}
