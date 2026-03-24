using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalStokvel.GroupService.Entities;

/// <summary>
/// Group member entity
/// </summary>
[Table("group_members")]
public class GroupMember
{
    /// <summary>
    /// Unique identifier for the membership
    /// </summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    /// <summary>
    /// Group ID
    /// </summary>
    [Required]
    [Column("group_id")]
    public Guid GroupId { get; set; }

    /// <summary>
    /// User ID
    /// </summary>
    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; }

    /// <summary>
    /// Member role in the group
    /// </summary>
    [Required]
    [Column("role")]
    public MemberRole Role { get; set; } = MemberRole.Member;

    /// <summary>
    /// Member status in the group
    /// </summary>
    [Required]
    [Column("status")]
    public MemberStatus Status { get; set; } = MemberStatus.Active;

    /// <summary>
    /// Timestamp when the member joined
    /// </summary>
    [Required]
    [Column("joined_at")]
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the member left (null if still active)
    /// </summary>
    [Column("left_at")]
    public DateTime? LeftAt { get; set; }

    /// <summary>
    /// User ID who invited this member
    /// </summary>
    [Column("invited_by")]
    public Guid? InvitedBy { get; set; }

    /// <summary>
    /// Invitation token (null after accepted)
    /// </summary>
    [Column("invite_token")]
    [MaxLength(100)]
    public string? InviteToken { get; set; }

    /// <summary>
    /// Timestamp when the invitation was accepted
    /// </summary>
    [Column("invite_accepted_at")]
    public DateTime? InviteAcceptedAt { get; set; }

    // Navigation properties
    
    /// <summary>
    /// The group this member belongs to
    /// </summary>
    [ForeignKey(nameof(GroupId))]
    public virtual Group? Group { get; set; }
}
