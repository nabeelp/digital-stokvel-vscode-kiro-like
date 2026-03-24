using System.ComponentModel.DataAnnotations;

namespace DigitalStokvel.GovernanceService.DTOs;

// ============================================================================
// VOTING REQUEST/RESPONSE DTOs
// ============================================================================

/// <summary>
/// Request to create a new vote in a group.
/// </summary>
public class CreateVoteRequest
{
    /// <summary>
    /// ID of the group for which the vote is being created.
    /// </summary>
    [Required]
    public Guid GroupId { get; set; }

    /// <summary>
    /// Subject or question being voted on.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Available voting options (minimum 2 required).
    /// </summary>
    [Required]
    [MinLength(2)]
    public List<string> Options { get; set; } = new();

    /// <summary>
    /// Duration of the voting period in hours.
    /// </summary>
    [Required]
    [Range(1, 720)] // 1 hour to 30 days
    public int VotingDurationHours { get; set; }
}

/// <summary>
/// Response after successfully creating a vote.
/// </summary>
public class CreateVoteResponse
{
    /// <summary>
    /// Unique ID of the created vote.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID of the group.
    /// </summary>
    public Guid GroupId { get; set; }

    /// <summary>
    /// Subject of the vote.
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Available voting options.
    /// </summary>
    public List<string> Options { get; set; } = new();

    /// <summary>
    /// Timestamp when voting period starts.
    /// </summary>
    public DateTime VotingStartsAt { get; set; }

    /// <summary>
    /// Timestamp when voting period ends.
    /// </summary>
    public DateTime VotingEndsAt { get; set; }

    /// <summary>
    /// Current status of the vote.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Number of votes required for quorum.
    /// </summary>
    public int QuorumRequired { get; set; }

    /// <summary>
    /// Current number of votes cast.
    /// </summary>
    public int CurrentVotes { get; set; }
}

/// <summary>
/// Request to cast a vote.
/// </summary>
public class CastVoteRequest
{
    /// <summary>
    /// The selected option from the available options.
    /// </summary>
    [Required]
    public string SelectedOption { get; set; } = string.Empty;
}

/// <summary>
/// Response after successfully casting a vote.
/// </summary>
public class CastVoteResponse
{
    /// <summary>
    /// Unique ID of the vote response.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID of the vote.
    /// </summary>
    public Guid VoteId { get; set; }

    /// <summary>
    /// The selected option.
    /// </summary>
    public string SelectedOption { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the vote was cast.
    /// </summary>
    public DateTime VotedAt { get; set; }
}

/// <summary>
/// Response containing vote status and results.
/// </summary>
public class VoteStatusResponse
{
    /// <summary>
    /// Unique ID of the vote.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID of the group.
    /// </summary>
    public Guid GroupId { get; set; }

    /// <summary>
    /// Subject of the vote.
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Available voting options.
    /// </summary>
    public List<string> Options { get; set; } = new();

    /// <summary>
    /// Current status of the vote.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when voting period starts.
    /// </summary>
    public DateTime VotingStartsAt { get; set; }

    /// <summary>
    /// Timestamp when voting period ends.
    /// </summary>
    public DateTime VotingEndsAt { get; set; }

    /// <summary>
    /// Vote results (option -> count mapping).
    /// </summary>
    public Dictionary<string, int> Results { get; set; } = new();

    /// <summary>
    /// Number of votes required for quorum.
    /// </summary>
    public int QuorumRequired { get; set; }

    /// <summary>
    /// Current number of votes cast.
    /// </summary>
    public int CurrentVotes { get; set; }

    /// <summary>
    /// Whether quorum has been reached.
    /// </summary>
    public bool QuorumReached { get; set; }
}

// ============================================================================
// DISPUTE REQUEST/RESPONSE DTOs
// ============================================================================

/// <summary>
/// Evidence item for a dispute.
/// </summary>
public class EvidenceItem
{
    /// <summary>
    /// Type of evidence (e.g., "image", "document", "text").
    /// </summary>
    [Required]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// URL or data URI for the evidence.
    /// </summary>
    [Required]
    public string Url { get; set; } = string.Empty;
}

/// <summary>
/// Request to raise a dispute.
/// </summary>
public class RaiseDisputeRequest
{
    /// <summary>
    /// ID of the group in which the dispute is being raised.
    /// </summary>
    [Required]
    public Guid GroupId { get; set; }

    /// <summary>
    /// Type of dispute (missed_payment, fraud, constitution_violation, other).
    /// </summary>
    [Required]
    public string DisputeType { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the dispute.
    /// </summary>
    [Required]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Evidence supporting the dispute (optional).
    /// </summary>
    public List<EvidenceItem> Evidence { get; set; } = new();
}

/// <summary>
/// Response after successfully raising a dispute.
/// </summary>
public class RaiseDisputeResponse
{
    /// <summary>
    /// Unique ID of the dispute.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID of the group.
    /// </summary>
    public Guid GroupId { get; set; }

    /// <summary>
    /// Type of dispute.
    /// </summary>
    public string DisputeType { get; set; } = string.Empty;

    /// <summary>
    /// Current status of the dispute.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the dispute was raised.
    /// </summary>
    public DateTime RaisedAt { get; set; }

    /// <summary>
    /// Deadline for resolution (typically 14 days from raised_at).
    /// </summary>
    public DateTime ResolutionDeadline { get; set; }
}

/// <summary>
/// Response containing dispute details.
/// </summary>
public class DisputeDetailsResponse
{
    /// <summary>
    /// Unique ID of the dispute.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID of the group.
    /// </summary>
    public Guid GroupId { get; set; }

    /// <summary>
    /// Type of dispute.
    /// </summary>
    public string DisputeType { get; set; } = string.Empty;

    /// <summary>
    /// Description of the dispute.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Evidence provided.
    /// </summary>
    public List<EvidenceItem> Evidence { get; set; } = new();

    /// <summary>
    /// Current status of the dispute.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Resolution notes (if resolved).
    /// </summary>
    public string? ResolutionNotes { get; set; }

    /// <summary>
    /// Timestamp when the dispute was raised.
    /// </summary>
    public DateTime RaisedAt { get; set; }

    /// <summary>
    /// Timestamp when the dispute was resolved (if resolved).
    /// </summary>
    public DateTime? ResolvedAt { get; set; }

    /// <summary>
    /// Information about who raised the dispute.
    /// </summary>
    public MemberDto RaisedBy { get; set; } = new();
}

/// <summary>
/// Member information DTO.
/// </summary>
public class MemberDto
{
    /// <summary>
    /// Unique ID of the member.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Name of the member.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Phone number of the member.
    /// </summary>
    public string Phone { get; set; } = string.Empty;
}
