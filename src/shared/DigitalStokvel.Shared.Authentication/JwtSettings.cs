namespace DigitalStokvel.Shared.Authentication;

/// <summary>
/// Configuration settings for JWT token generation and validation
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// Secret key used for signing tokens. Must be at least 32 characters.
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Token issuer (stokvel-api)
    /// </summary>
    public string Issuer { get; set; } = "stokvel-api";

    /// <summary>
    /// Token audience (stokvel-clients)
    /// </summary>
    public string Audience { get; set; } = "stokvel-clients";

    /// <summary>
    /// Access token expiration in minutes (default: 1440 = 24 hours)
    /// </summary>
    public int ExpirationMinutes { get; set; } = 1440;

    /// <summary>
    /// Refresh token expiration in days (default: 30 days)
    /// </summary>
    public int RefreshTokenExpirationDays { get; set; } = 30;
}
