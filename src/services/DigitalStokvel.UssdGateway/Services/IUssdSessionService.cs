namespace DigitalStokvel.UssdGateway.Services;

/// <summary>
/// Service for managing USSD sessions in Redis
/// Sessions expire after 120 seconds per design requirements
/// </summary>
public interface IUssdSessionService
{
    /// <summary>
    /// Gets or creates a USSD session
    /// </summary>
    Task<Entities.UssdSession> GetOrCreateSessionAsync(string sessionId, string phoneNumber, string language);

    /// <summary>
    /// Updates an existing USSD session
    /// </summary>
    Task UpdateSessionAsync(Entities.UssdSession session);

    /// <summary>
    /// Terminates a USSD session
    /// </summary>
    Task TerminateSessionAsync(string sessionId);

    /// <summary>
    /// Cleans up expired sessions (older than 120 seconds)
    /// </summary>
    Task CleanupExpiredSessionsAsync();
}
