using DigitalStokvel.ContributionService.DTOs;

namespace DigitalStokvel.ContributionService.Services;

/// <summary>
/// Service interface for contribution operations
/// </summary>
public interface IContributionService
{
    /// <summary>
    /// Create a new contribution
    /// </summary>
    Task<CreateContributionResponse> CreateContributionAsync(CreateContributionRequest request, Guid userId);

    /// <summary>
    /// Get contribution history for a group
    /// </summary>
    Task<ContributionHistoryResponse> GetGroupContributionsAsync(Guid groupId, Guid? memberId, DateTime? fromDate, DateTime? toDate, int page, int limit);

    /// <summary>
    /// Set up a recurring payment for a member
    /// </summary>
    Task<CreateRecurringPaymentResponse> CreateRecurringPaymentAsync(CreateRecurringPaymentRequest request, Guid userId);

    /// <summary>
    /// Get user's recurring payments
    /// </summary>
    Task<RecurringPaymentsResponse> GetUserRecurringPaymentsAsync(Guid userId);
}
