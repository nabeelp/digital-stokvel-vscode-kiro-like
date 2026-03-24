using DigitalStokvel.PayoutService.Entities;
using Microsoft.EntityFrameworkCore;

namespace DigitalStokvel.PayoutService.Data;

/// <summary>
/// Entity Framework Core database context for the Payout Service.
/// Manages payouts and payout disbursements.
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Payouts initiated for groups.
    /// </summary>
    public DbSet<Payout> Payouts { get; set; } = null!;

    /// <summary>
    /// Individual disbursements for approved payouts.
    /// </summary>
    public DbSet<PayoutDisbursement> PayoutDisbursements { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ====================================================================
        // PAYOUT ENTITY CONFIGURATION
        // ====================================================================

        modelBuilder.Entity<Payout>(entity =>
        {
            // Primary key
            entity.HasKey(e => e.Id);

            // Enum to string conversion for PostgreSQL compatibility
            entity.Property(e => e.PayoutType)
                .HasConversion<string>();

            entity.Property(e => e.Status)
                .HasConversion<string>();

            // Indexes
            entity.HasIndex(e => e.GroupId)
                .HasDatabaseName("idx_payouts_group_id");

            entity.HasIndex(e => e.Status)
                .HasDatabaseName("idx_payouts_status");

            entity.HasIndex(e => e.PayoutType)
                .HasDatabaseName("idx_payouts_payout_type");

            entity.HasIndex(e => e.InitiatedBy)
                .HasDatabaseName("idx_payouts_initiated_by");

            entity.HasIndex(e => e.ApprovedBy)
                .HasDatabaseName("idx_payouts_approved_by")
                .HasFilter("approved_by IS NOT NULL");

            entity.HasIndex(e => e.InitiatedAt)
                .HasDatabaseName("idx_payouts_initiated_at")
                .IsDescending();

            entity.HasIndex(e => e.ExecutedAt)
                .HasDatabaseName("idx_payouts_executed_at")
                .HasFilter("executed_at IS NOT NULL")
                .IsDescending();

            // Index for pending approvals
            entity.HasIndex(e => new { e.GroupId, e.InitiatedAt })
                .HasDatabaseName("idx_payouts_pending_approval")
                .HasFilter("status = 'pending_approval'")
                .IsDescending(false, true);

            // Navigation property
            entity.HasMany(p => p.Disbursements)
                .WithOne(d => d.Payout)
                .HasForeignKey(d => d.PayoutId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ====================================================================
        // PAYOUT DISBURSEMENT ENTITY CONFIGURATION
        // ====================================================================

        modelBuilder.Entity<PayoutDisbursement>(entity =>
        {
            // Primary key
            entity.HasKey(e => e.Id);

            // Enum to string conversion for PostgreSQL compatibility
            entity.Property(e => e.Status)
                .HasConversion<string>();

            // Indexes
            entity.HasIndex(e => e.PayoutId)
                .HasDatabaseName("idx_disbursements_payout_id");

            entity.HasIndex(e => e.MemberId)
                .HasDatabaseName("idx_disbursements_member_id");

            entity.HasIndex(e => e.Status)
                .HasDatabaseName("idx_disbursements_status");

            entity.HasIndex(e => e.TransactionRef)
                .HasDatabaseName("idx_disbursements_transaction_ref")
                .HasFilter("transaction_ref IS NOT NULL")
                .IsUnique();

            entity.HasIndex(e => e.ExecutedAt)
                .HasDatabaseName("idx_disbursements_executed_at")
                .HasFilter("executed_at IS NOT NULL")
                .IsDescending();

            // Index for pending disbursements
            entity.HasIndex(e => new { e.PayoutId, e.Status })
                .HasDatabaseName("idx_disbursements_pending")
                .HasFilter("status IN ('pending_approval', 'approved')");

            // Unique constraint: one disbursement per member per payout
            entity.HasIndex(e => new { e.PayoutId, e.MemberId })
                .HasDatabaseName("unique_member_per_payout")
                .IsUnique();
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
            if (entry.Entity is Payout payout)
            {
                payout.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is PayoutDisbursement disbursement)
            {
                disbursement.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
