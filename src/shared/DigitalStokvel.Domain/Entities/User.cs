using DigitalStokvel.Domain.Common;
using DigitalStokvel.Domain.Enums;

namespace DigitalStokvel.Domain.Entities;

/// <summary>
/// Represents a user in the Digital Stokvel platform
/// </summary>
public class User : BaseEntity
{
    /// <summary>
    /// Phone number (unique identifier for users)
    /// </summary>
    public required string PhoneNumber { get; set; }

    /// <summary>
    /// South African ID number (encrypted at rest)
    /// </summary>
    public required string IdNumber { get; set; }

    /// <summary>
    /// First name
    /// </summary>
    public required string FirstName { get; set; }

    /// <summary>
    /// Last name
    /// </summary>
    public required string LastName { get; set; }

    /// <summary>
    /// Preferred language for communication
    /// </summary>
    public Language PreferredLanguage { get; set; } = Language.English;

    /// <summary>
    /// PIN hash for authentication (encrypted)
    /// </summary>
    public string? PinHash { get; set; }

    /// <summary>
    /// Email address (optional)
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Indicates if user has completed onboarding
    /// </summary>
    public bool IsOnboarded { get; set; }

    /// <summary>
    /// Last login timestamp
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Group memberships
    /// </summary>
    public ICollection<GroupMember> GroupMemberships { get; set; } = new List<GroupMember>();

    /// <summary>
    /// Full name of the user
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";
}
