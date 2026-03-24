using DigitalStokvel.GovernanceService.DTOs;

namespace DigitalStokvel.GovernanceService.Services;

/// <summary>
/// Service interface for governance operations (voting and disputes).
/// </summary>
public interface IGovernanceService
{
    /// <summary>
    /// Creates a new vote for a group.
    /// </summary>
    Task<CreateVoteResponse> CreateVoteAsync(CreateVoteRequest request, Guid userId);

    /// <summary>
    /// Casts a vote for a member.
    /// </summary>
    Task<CastVoteResponse> CastVoteAsync(Guid voteId, CastVoteRequest request, Guid userId);

    /// <summary>
    /// Retrieves vote status including results and quorum information.
    /// </summary>
    Task<VoteStatusResponse> GetVoteStatusAsync(Guid voteId);

    /// <summary>
    /// Raises a dispute for a group.
    /// </summary>
    Task<RaiseDisputeResponse> RaiseDisputeAsync(RaiseDisputeRequest request, Guid userId);

    /// <summary>
    /// Retrieves dispute details.
    /// </summary>
    Task<DisputeDetailsResponse> GetDisputeDetailsAsync(Guid disputeId);
}
