using DigitalStokvel.UssdGateway.Entities;
using Microsoft.EntityFrameworkCore;

namespace DigitalStokvel.UssdGateway.Data;

/// <summary>
/// Database context for USSD Gateway Service
/// Manages USSD session persistence and tracking
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// USSD sessions for tracking user interactions
    /// Sessions expire after 120 seconds per design requirements
    /// </summary>
    public DbSet<UssdSession> UssdSessions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure UssdSession entity
        modelBuilder.Entity<UssdSession>(entity =>
        {
            entity.HasKey(e => e.SessionId);
            
            entity.HasIndex(e => e.PhoneNumber)
                .HasDatabaseName("idx_ussd_sessions_phone_number");
            
            entity.HasIndex(e => e.ExpiresAt)
                .HasDatabaseName("idx_ussd_sessions_expires_at");
            
            entity.HasIndex(e => new { e.PhoneNumber, e.IsTerminated })
                .HasDatabaseName("idx_ussd_sessions_phone_terminated");

            // Configure jsonb column for PostgreSQL
            entity.Property(e => e.Context)
                .HasColumnType("jsonb");
        });
    }
}
