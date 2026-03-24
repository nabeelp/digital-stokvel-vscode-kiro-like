using DigitalStokvel.NotificationService.Entities;
using Microsoft.EntityFrameworkCore;

namespace DigitalStokvel.NotificationService.Data;

/// <summary>
/// Entity Framework Core database context for the Notification Service.
/// Manages notifications and delivery tracking.
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Multi-channel notifications with delivery tracking.
    /// </summary>
    public DbSet<Notification> Notifications { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ====================================================================
        // NOTIFICATION ENTITY CONFIGURATION
        // ====================================================================

        modelBuilder.Entity<Notification>(entity =>
        {
            // Primary key
            entity.HasKey(e => e.Id);

            // Enum to string conversion for PostgreSQL compatibility
            entity.Property(e => e.Channel)
                .HasConversion<string>();

            entity.Property(e => e.Status)
                .HasConversion<string>();

            // Indexes (matching V006 migration)
            entity.HasIndex(e => new { e.RecipientId, e.Status })
                .HasDatabaseName("idx_notifications_recipient_status");

            entity.HasIndex(e => new { e.RecipientId, e.CreatedAt })
                .HasDatabaseName("idx_notifications_recipient_created")
                .IsDescending(false, true);

            entity.HasIndex(e => e.Status)
                .HasDatabaseName("idx_notifications_pending")
                .HasFilter("status = 'pending'")
                .IsDescending(false);

            entity.HasIndex(e => new { e.Status, e.RetryCount, e.CreatedAt })
                .HasDatabaseName("idx_notifications_failed_retry")
                .HasFilter("status = 'failed' AND retry_count < 3")
                .IsDescending(false, false, false);

            entity.HasIndex(e => new { e.Channel, e.Status })
                .HasDatabaseName("idx_notifications_channel_status");

            entity.HasIndex(e => e.SentAt)
                .HasDatabaseName("idx_notifications_sent_at")
                .HasFilter("sent_at IS NOT NULL")
                .IsDescending();

            entity.HasIndex(e => e.TemplateKey)
                .HasDatabaseName("idx_notifications_template_key");
        });
    }
}
