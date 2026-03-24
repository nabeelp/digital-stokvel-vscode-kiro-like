using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalStokvel.GovernanceService.Entities;

/// <summary>
/// Represents an individual member's vote response.
/// </summary>
[Table("vote_responses")]
public class VoteResponse
{
    /// <summary>
    /// Unique identifier for the vote response.
    /// </summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// ID of the vote this response belongs to.
    /// </summary>
    [Required]
    [Column("vote_id")]
    public Guid VoteId { get; set; }

    /// <summary>
    /// ID of the group member who cast this vote.
    /// </summary>
    [Required]
    [Column("member_id")]
    public Guid MemberId { get; set; }

    /// <summary>
    /// The option selected by the member.
    /// Must match one of the options in the parent Vote.
    /// </summary>
    [Required]
    [Column("selected_option")]
    [MaxLength(100)]
    public string SelectedOption { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the vote was cast.
    /// </summary>
    [Required]
    [Column("voted_at")]
    public DateTime VotedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the vote response record was created.
    /// </summary>
    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the vote response record was last updated.
    /// </summary>
    [Required]
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property to the parent vote.
    /// </summary>
    public Vote? Vote { get; set; }
}
