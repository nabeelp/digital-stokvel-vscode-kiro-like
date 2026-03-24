using DigitalStokvel.UssdGateway.DTOs;

namespace DigitalStokvel.UssdGateway.Services;

/// <summary>
/// Service for communicating with API Gateway
/// Handles user authentication, group data, and contribution processing
/// </summary>
public interface IApiGatewayClient
{
    /// <summary>
    /// Gets the user's groups from API Gateway
    /// </summary>
    Task<UserGroupsResponseDto?> GetUserGroupsAsync(string phoneNumber);

    /// <summary>
    /// Gets the contribution amount due for a specific group member
    /// </summary>
    Task<ContributionDueResponseDto?> GetContributionDueAsync(Guid groupId, string phoneNumber);

    /// <summary>
    /// Processes a contribution payment through API Gateway
    /// </summary>
    Task<ContributionPaymentResponseDto?> ProcessContributionAsync(ContributionPaymentRequestDto request, string phoneNumber);
}
