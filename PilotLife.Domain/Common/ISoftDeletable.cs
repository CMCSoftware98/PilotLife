namespace PilotLife.Domain.Common;

/// <summary>
/// Interface for entities that support soft deletion.
/// Soft-deleted entities are not physically removed from the database.
/// </summary>
public interface ISoftDeletable
{
    /// <summary>
    /// When the entity was soft deleted. Null if not deleted.
    /// </summary>
    DateTimeOffset? DeletedAt { get; set; }

    /// <summary>
    /// The ID of the user who deleted this entity.
    /// </summary>
    Guid? DeletedBy { get; set; }

    /// <summary>
    /// Whether this entity has been soft deleted.
    /// </summary>
    bool IsDeleted { get; }
}
