using DigitalStokvel.ContributionService.Entities;
using Microsoft.EntityFrameworkCore;

namespace DigitalStokvel.ContributionService.Data;

/// <summary>
/// Entity Framework DbContext for Contribution Service
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Contributions DbSet
    /// </summary>
    public DbSet<Contribution> Contributions { get; set; }

    /// <summary>
    /// Contribution Ledger DbSet (immutable append-only)
    /// </summary>
    public DbSet<ContributionLedger> ContributionLedger { get; set; }

    /// <summary>
    /// Recurring Payments DbSet
    /// </summary>
    public DbSet<RecurringPayment> RecurringPayments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Contribution entity
        modelBuilder.Entity<Contribution>(entity =>
        {
            // Enum to string conversions for PostgreSQL
            entity.Property(c => c.Status)
                .HasConversion<string>()
                .HasColumnType("varchar(20)");

            // Unique index on transaction_ref
            entity.HasIndex(c => c.TransactionRef)
                .IsUnique()
                .HasFilter("transaction_ref IS NOT NULL");

            // Indexes for performance
            entity.HasIndex(c => c.GroupId);
            entity.HasIndex(c => c.MemberId);
            entity.HasIndex(c => c.Status);
            entity.HasIndex(c => c.DueDate);
            entity.HasIndex(c => new { c.GroupId, c.DueDate })
                .HasFilter("status = 'pending' AND due_date < CURRENT_DATE"); // Overdue contributions

            // Check constraints are handled by database migration
        });

        // Configure ContributionLedger entity
        modelBuilder.Entity<ContributionLedger>(entity =>
        {
            // Indexes for ledger queries
            entity.HasIndex(l => l.ContributionId);
            entity.HasIndex(l => new { l.GroupId, l.RecordedAt });
            entity.HasIndex(l => new { l.MemberId, l.RecordedAt });
            entity.HasIndex(l => l.TransactionRef);
            entity.HasIndex(l => l.RecordedAt);

            // Relationship to Contribution
            entity.HasOne(l => l.Contribution)
                .WithMany()
                .HasForeignKey(l => l.ContributionId)
                .OnDelete(DeleteBehavior.Restrict);// RESTRICT to prevent deletion of referenced contributions
        });

        // Configure RecurringPayment entity
        modelBuilder.Entity<RecurringPayment>(entity =>
        {
            // Enum to string conversions
            entity.Property(r => r.Frequency)
                .HasConversion<string>()
                .HasColumnType("varchar(20)");

            entity.Property(r => r.Status)
                .HasConversion<string>()
                .HasColumnType("varchar(20)");

            // Partial unique index for active recurring payments only
            entity.HasIndex(r => new { r.MemberId, r.GroupId })
                .IsUnique()
                .HasFilter("status = 'active'");

            // Indexes for performance
            entity.HasIndex(r => r.MemberId);
            entity.HasIndex(r => r.GroupId);
            entity.HasIndex(r => r.Status);
            entity.HasIndex(r => r.NextPaymentDate)
                .HasFilter("status = 'active'");
            entity.HasIndex(r => new { r.NextPaymentDate, r.Status })
                .HasFilter("status = 'active' AND next_payment_date <= CURRENT_DATE"); // Due recurring payments
        });
    }

    /// <summary>
    /// Override SaveChanges to automatically update UpdatedAt timestamps
    /// </summary>
    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    /// <summary>
    /// Override SaveChangesAsync to automatically update UpdatedAt timestamps
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity is Contribution contribution)
            {
                contribution.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is RecurringPayment recurringPayment)
            {
                recurringPayment.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
