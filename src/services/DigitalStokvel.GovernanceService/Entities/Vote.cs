using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalStokvel.GovernanceService.Entities;

/// <summary>
/// Represents a vote within a stokvel group for democratic decision-making.
/// </summary>
[Table("votes")]
public class Vote
{
    /// <summary>
    /// Unique identifier for the vote.
    /// </summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// ID of the group this vote belongs to.
    /// </summary>
    [Required]
    [Column("group_id")]
    public Guid GroupId { get; set; }

    /// <summary>
    /// ID of the user who initiated the vote.
    /// </summary>
    [Required]
    [Column("initiated_by")]
    public Guid InitiatedBy { get; set; }

    /// <summary>
    /// Subject or question being voted on.
    /// </summary>
    [Required]
    [Column("subject")]
    [MaxLength(200)]
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Available voting options as JSON array.
    /// Example: ["approve", "reject"] or ["option1", "option2", "option3"]
    /// </summary>
    [Required]
    [Column("options", TypeName = "jsonb")]
    public string Options { get; set; } = "[]";

    /// <summary>
    /// Timestamp when voting period starts.
    /// </summary>
    [Required]
    [Column("voting_starts_at")]
    public DateTime VotingStartsAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when voting period ends.
    /// Must be after VotingStartsAt.
    /// </summary>
    [Required]
    [Column("voting_ends_at")]
    public DateTime VotingEndsAt { get; set; }

    /// <summary>
    /// Current status of the vote (draft, active, closed).
    /// </summary>
    [Required]
    [Column("status")]
    public VoteStatus Status { get; set; } = VoteStatus.Draft;

    /// <summary>
    /// Vote results stored as JSON object.
    /// Example: {"approve": 12, "reject": 3}
    /// </summary>
    [Column("results", TypeName = "jsonb")]
    public string? Results { get; set; } = "{}";

    /// <summary>
    /// Timestamp when the vote record was created.
    /// </summary>
    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the vote record was last updated.
    /// </summary>
    [Required]
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property for vote responses.
    /// </summary>
    public ICollection<VoteResponse> Responses { get; set; } = new List<VoteResponse>();
}
