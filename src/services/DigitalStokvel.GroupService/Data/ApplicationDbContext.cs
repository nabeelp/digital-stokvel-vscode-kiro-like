using DigitalStokvel.GroupService.Entities;
using Microsoft.EntityFrameworkCore;

namespace DigitalStokvel.GroupService.Data;

/// <summary>
/// Application database context for Group Service
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Groups table
    /// </summary>
    public DbSet<Group> Groups { get; set; } = null!;

    /// <summary>
    /// Group members table
    /// </summary>
    public DbSet<GroupMember> GroupMembers { get; set; } = null!;

    /// <summary>
    /// Group savings accounts table
    /// </summary>
    public DbSet<GroupSavingsAccount> GroupSavingsAccounts { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Group entity
        modelBuilder.Entity<Group>(entity =>
        {
            entity.ToTable("groups");
            
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.GroupType)
                .HasConversion<string>()
                .HasColumnName("group_type");
            
            entity.Property(e => e.ContributionFrequency)
                .HasConversion<string>()
                .HasColumnName("contribution_frequency");
            
            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasColumnName("status");

            // Indexes
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.GroupType);
            entity.HasIndex(e => e.CreatedBy);
        });

        // Configure GroupMember entity
        modelBuilder.Entity<GroupMember>(entity =>
        {
            entity.ToTable("group_members");
            
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Role)
                .HasConversion<string>()
                .HasColumnName("role");
            
            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasColumnName("status");

            // Unique constraint: one user per group
            entity.HasIndex(e => new { e.GroupId, e.UserId })
                .IsUnique();

            // Indexes
            entity.HasIndex(e => e.GroupId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Role);
            entity.HasIndex(e => e.Status);

            // Relationship with Group
            entity.HasOne(e => e.Group)
                .WithMany(g => g.Members)
                .HasForeignKey(e => e.GroupId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure GroupSavingsAccount entity
        modelBuilder.Entity<GroupSavingsAccount>(entity =>
        {
            entity.ToTable("group_savings_accounts");
            
            entity.HasKey(e => e.Id);

            // Unique constraint: one account per group
            entity.HasIndex(e => e.GroupId)
                .IsUnique();
            
            entity.HasIndex(e => e.AccountNumber)
                .IsUnique();

            // Relationship with Group
            entity.HasOne(e => e.Group)
                .WithOne(g => g.SavingsAccount)
                .HasForeignKey<GroupSavingsAccount>(e => e.GroupId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

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
            if (entry.Entity is Group group)
            {
                group.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is GroupSavingsAccount account)
            {
                account.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
