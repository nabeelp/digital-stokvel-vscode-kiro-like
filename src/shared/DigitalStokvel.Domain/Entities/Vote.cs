using DigitalStokvel.Domain.Common;
using DigitalStokvel.Domain.Enums;

namespace DigitalStokvel.Domain.Entities;

/// <summary>
/// Represents a vote on a governance decision in a group
/// </summary>
public class Vote : BaseEntity
{
    /// <summary>
    /// Reference to the group
    /// </summary>
    public required Guid GroupId { get; set; }

    /// <summary>
    /// Title of the vote
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Description of what is being voted on
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// Type of vote
    /// </summary>
    public VoteType VoteType { get; set; }

    /// <summary>
    /// Member who initiated the vote
    /// </summary>
    public required Guid InitiatedBy { get; set; }

    /// <summary>
    /// Vote deadline
    /// </summary>
    public DateTime Deadline { get; set; }

    /// <summary>
    /// Whether the vote has been closed
    /// </summary>
    public bool IsClosed { get; set; }

    /// <summary>
    /// Timestamp when vote was closed
    /// </summary>
    public DateTime? ClosedAt { get; set; }

    /// <summary>
    /// Whether the vote passed
    /// </summary>
    public bool? Passed { get; set; }

    /// <summary>
    /// Navigation property to Group
    /// </summary>
    public Group? Group { get; set; }

    /// <summary>
    /// Individual vote responses from members
    /// </summary>
    public ICollection<VoteResponse> Responses { get; set; } = new List<VoteResponse>();

    /// <summary>
    /// Check if vote is still active
    /// </summary>
    public bool IsActive => !IsClosed && Deadline > DateTime.UtcNow;
}
