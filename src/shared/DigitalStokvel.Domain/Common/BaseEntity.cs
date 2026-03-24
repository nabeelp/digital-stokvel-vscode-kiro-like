namespace DigitalStokvel.Domain.Common;

/// <summary>
/// Base entity class with common properties for all domain entities
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Timestamp when entity was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when entity was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    protected BaseEntity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Base class for entities that can be soft-deleted
/// </summary>
public abstract class SoftDeletableEntity : BaseEntity
{
    /// <summary>
    /// Indicates if the entity has been soft-deleted
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Timestamp when entity was deleted
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// User who deleted the entity
    /// </summary>
    public Guid? DeletedBy { get; set; }

    /// <summary>
    /// Mark entity as deleted
    /// </summary>
    public virtual void SoftDelete(Guid deletedBy)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Restore a soft-deleted entity
    /// </summary>
    public virtual void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = null;
        UpdatedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Base class for audit trail entities (immutable, append-only)
/// </summary>
public abstract class AuditEntity
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Timestamp when record was created (immutable)
    /// </summary>
    public DateTime RecordedAt { get; init; }

    protected AuditEntity()
    {
        Id = Guid.NewGuid();
        RecordedAt = DateTime.UtcNow;
    }
}
