using DigitalStokvel.UssdGateway.DTOs;

namespace DigitalStokvel.UssdGateway.Services;

/// <summary>
/// Service for handling USSD menu navigation and business logic
/// Implements 3-level menu system per design constraints
/// </summary>
public interface IUssdMenuService
{
    /// <summary>
    /// Processes a USSD request and returns appropriate menu response
    /// </summary>
    Task<UssdSessionResponseDto> ProcessRequestAsync(UssdSessionRequestDto request, Entities.UssdSession session);

    /// <summary>
    /// Renders the main menu for authenticated users
    /// </summary>
    Task<UssdSessionResponseDto> ShowMainMenuAsync(Entities.UssdSession session);

    /// <summary>
    /// Renders the authentication menu for unauthenticated users
    /// </summary>
    Task<UssdSessionResponseDto> ShowAuthenticationMenuAsync(Entities.UssdSession session);
}
