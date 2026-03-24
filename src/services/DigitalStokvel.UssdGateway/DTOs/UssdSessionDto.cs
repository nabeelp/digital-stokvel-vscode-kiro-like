using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DigitalStokvel.UssdGateway.DTOs;

/// <summary>
/// USSD session request from aggregator
/// Based on design.md Section 5.6 USSD API Integration
/// </summary>
public class UssdSessionRequestDto
{
    /// <summary>
    /// Unique session identifier from USSD aggregator
    /// </summary>
    [Required]
    [JsonPropertyName("session_id")]
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// User's phone number in E.164 format
    /// </summary>
    [Required]
    [JsonPropertyName("phone_number")]
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// USSD string dialed by user (e.g., *120*7878#)
    /// </summary>
    [Required]
    [JsonPropertyName("ussd_string")]
    public string UssdString { get; set; } = string.Empty;

    /// <summary>
    /// User's input at current menu level (null for initial request)
    /// </summary>
    [JsonPropertyName("user_input")]
    public string? UserInput { get; set; }

    /// <summary>
    /// Preferred language code (en, zu, xh, st, af)
    /// Defaults to 'en' if not provided
    /// </summary>
    [JsonPropertyName("language")]
    public string Language { get; set; } = "en";
}

/// <summary>
/// USSD session response to aggregator
/// Based on design.md Section 5.6 USSD API Integration
/// </summary>
public class UssdSessionResponseDto
{
    /// <summary>
    /// Session identifier
    /// </summary>
    [JsonPropertyName("session_id")]
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// Response type:
    /// - CON: Continue session (show menu and wait for input)
    /// - END: End session (display final message)
    /// </summary>
    [Required]
    [JsonPropertyName("response_type")]
    public string ResponseType { get; set; } = "CON";

    /// <summary>
    /// Message to display to user
    /// Includes menu options for CON responses
    /// </summary>
    [Required]
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Current session state for tracking navigation
    /// </summary>
    [JsonPropertyName("session_state")]
    public UssdSessionStateDto? SessionState { get; set; }
}

/// <summary>
/// Session state tracking for menu navigation
/// </summary>
public class UssdSessionStateDto
{
    /// <summary>
    /// Current menu level (1-3, design constraint: max 3 levels)
    /// </summary>
    [JsonPropertyName("menu_level")]
    public int MenuLevel { get; set; }

    /// <summary>
    /// Context object for current action
    /// Stores action type, selected group, payment details, etc.
    /// </summary>
    [JsonPropertyName("context")]
    public Dictionary<string, object> Context { get; set; } = new();
}
