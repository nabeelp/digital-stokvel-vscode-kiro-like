namespace DigitalStokvel.Common.Constants;

/// <summary>
/// Application-wide constants
/// </summary>
public static class AppConstants
{
    /// <summary>
    /// Default pagination page size
    /// </summary>
    public const int DefaultPageSize = 20;

    /// <summary>
    /// Maximum pagination page size
    /// </summary>
    public const int MaxPageSize = 100;

    /// <summary>
    /// JWT token expiration in minutes
    /// </summary>
    public const int JwtExpirationMinutes = 60;

    /// <summary>
    /// Refresh token expiration in days
    /// </summary>
    public const int RefreshTokenExpirationDays = 30;

    /// <summary>
    /// Maximum group size
    /// </summary>
    public const int MaxGroupMembers = 50;

    /// <summary>
    /// USSD session timeout in seconds
    /// </summary>
    public const int UssdSessionTimeoutSeconds = 120;

    /// <summary>
    /// Maximum SMS rate per user per day
    /// </summary>
    public const int MaxSmsPerUserPerDay = 10;

    /// <summary>
    /// API rate limit per user per minute
    /// </summary>
    public const int ApiRateLimitPerMinute = 100;
}

/// <summary>
/// Cache key constants
/// </summary>
public static class CacheKeys
{
    public const string UserPrefix = "user:";
    public const string GroupPrefix = "group:";
    public const string UssdSessionPrefix = "ussd:session:";

    public static string GetUserKey(Guid userId) => $"{UserPrefix}{userId}";
    public static string GetGroupKey(Guid groupId) => $"{GroupPrefix}{groupId}";
    public static string GetUssdSessionKey(string sessionId) => $"{UssdSessionPrefix}{sessionId}";
}

/// <summary>
/// Error message constants
/// </summary>
public static class ErrorMessages
{
    public const string GroupNotFound = "Group not found";
    public const string UserNotFound = "User not found";
    public const string UnauthorizedAccess = "Unauthorized access to this resource";
    public const string InvalidCredentials = "Invalid credentials";
    public const string GroupAtCapacity = "Group has reached maximum capacity";
    public const string InsufficientFunds = "Insufficient funds for this operation";
    public const string PaymentFailed = "Payment processing failed";
    public const string DuplicatePhoneNumber = "Phone number already registered";
    public const string InvalidPhoneNumber = "Invalid phone number format";
    public const string InvalidIdNumber = "Invalid ID number format";
}

/// <summary>
/// Supported languages
/// </summary>
public static class SupportedLanguages
{
    public static readonly string[] All = { "en", "zu", "xh", "af", "st" };

    public const string English = "en";
    public const string Zulu = "zu";
    public const string Xhosa = "xh";
    public const string Afrikaans = "af";
    public const string Sesotho = "st";
}
