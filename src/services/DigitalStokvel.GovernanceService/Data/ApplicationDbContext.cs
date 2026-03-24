using DigitalStokvel.GovernanceService.Entities;
using Microsoft.EntityFrameworkCore;

namespace DigitalStokvel.GovernanceService.Data;

/// <summary>
/// Entity Framework Core database context for the Governance Service.
/// Manages votes, vote responses, and disputes.
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Votes for group decision-making.
    /// </summary>
    public DbSet<Vote> Votes { get; set; } = null!;

    /// <summary>
    /// Individual member vote responses.
    /// </summary>
    public DbSet<VoteResponse> VoteResponses { get; set; } = null!;

    /// <summary>
    /// Disputes raised by members for resolution.
    /// </summary>
    public DbSet<Dispute> Disputes { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ====================================================================
        // VOTE ENTITY CONFIGURATION
        // ====================================================================

        modelBuilder.Entity<Vote>(entity =>
        {
            // Primary key
            entity.HasKey(e => e.Id);

            // Enum to string conversion for PostgreSQL compatibility
            entity.Property(e => e.Status)
                .HasConversion<string>();

            // Indexes
            entity.HasIndex(e => new { e.GroupId, e.Status })
                .HasDatabaseName("idx_votes_group_status");

            entity.HasIndex(e => new { e.GroupId, e.CreatedAt })
                .HasDatabaseName("idx_votes_group_created")
                .IsDescending(false, true);

            entity.HasIndex(e => e.InitiatedBy)
                .HasDatabaseName("idx_votes_initiated_by");

            entity.HasIndex(e => e.VotingEndsAt)
                .HasDatabaseName("idx_votes_ending_soon")
                .HasFilter("status = 'active'");

            entity.HasIndex(e => e.Status)
                .HasDatabaseName("idx_votes_status");

            // Navigation property
            entity.HasMany(v => v.Responses)
                .WithOne(r => r.Vote)
                .HasForeignKey(r => r.VoteId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ====================================================================
        // VOTE RESPONSE ENTITY CONFIGURATION
        // ====================================================================

        modelBuilder.Entity<VoteResponse>(entity =>
        {
            // Primary key
            entity.HasKey(e => e.Id);

            // Indexes
            entity.HasIndex(e => e.VoteId)
                .HasDatabaseName("idx_vote_responses_vote_id");

            entity.HasIndex(e => e.MemberId)
                .HasDatabaseName("idx_vote_responses_member_id");

            entity.HasIndex(e => e.VotedAt)
                .HasDatabaseName("idx_vote_responses_voted_at")
                .IsDescending();

            // Unique constraint: one vote per member per vote
            entity.HasIndex(e => new { e.VoteId, e.MemberId })
                .HasDatabaseName("unique_member_vote")
                .IsUnique();
        });

        // ====================================================================
        // DISPUTE ENTITY CONFIGURATION
        // ====================================================================

        modelBuilder.Entity<Dispute>(entity =>
        {
            // Primary key
            entity.HasKey(e => e.Id);

            // Enum to string conversion for PostgreSQL compatibility
            entity.Property(e => e.DisputeType)
                .HasConversion<string>();

            entity.Property(e => e.Status)
                .HasConversion<string>();

            // Indexes
            entity.HasIndex(e => new { e.GroupId, e.Status })
                .HasDatabaseName("idx_disputes_group_status");

            entity.HasIndex(e => new { e.GroupId, e.RaisedAt })
                .HasDatabaseName("idx_disputes_group_raised_at")
                .IsDescending(false, true);

            entity.HasIndex(e => e.RaisedBy)
                .HasDatabaseName("idx_disputes_raised_by");

            entity.HasIndex(e => e.Status)
                .HasDatabaseName("idx_disputes_status");

            entity.HasIndex(e => e.ResolvedAt)
                .HasDatabaseName("idx_disputes_resolved_at")
                .HasFilter("resolved_at IS NOT NULL")
                .IsDescending();

            entity.HasIndex(e => e.RaisedAt)
                .HasDatabaseName("idx_disputes_raised_at")
                .IsDescending();
        });
    }

    /// <summary>
    /// Override SaveChanges to automatically update UpdatedAt timestamp.
    /// </summary>
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        UpdateTimestamps();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    /// <summary>
    /// Override SaveChangesAsync to automatically update UpdatedAt timestamp.
    /// </summary>
    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    /// <summary>
    /// Updates the UpdatedAt timestamp for modified entities.
    /// </summary>
    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity is Vote vote)
            {
                vote.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is VoteResponse voteResponse)
            {
                voteResponse.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is Dispute dispute)
            {
                dispute.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
