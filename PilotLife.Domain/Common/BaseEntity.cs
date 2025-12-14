using System.ComponentModel.DataAnnotations.Schema;
using PilotLife.Domain.Extensions;

namespace PilotLife.Domain.Common;

/// <summary>
/// Base class for all domain entities. Provides UUID v7 identifier and audit timestamps.
/// All entities should inherit from this class.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Unique identifier using UUID v7 (time-ordered for better database performance).
    /// </summary>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// When the entity was created. Set automatically on instantiation.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// When the entity was last modified. Null if never modified after creation.
    /// </summary>
    public DateTimeOffset? ModifiedAt { get; set; }

    /// <summary>
    /// Gets the creation timestamp extracted from the UUID v7 identifier.
    /// Useful for verification or when CreatedAt wasn't explicitly set.
    /// </summary>
    [NotMapped]
    public DateTimeOffset IdTimestamp => Id.GetTimestamp();
}
