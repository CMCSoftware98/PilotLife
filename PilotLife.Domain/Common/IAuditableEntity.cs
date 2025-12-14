namespace PilotLife.Domain.Common;

/// <summary>
/// Interface for entities that track who created and modified them.
/// </summary>
public interface IAuditableEntity
{
    /// <summary>
    /// The ID of the user who created this entity.
    /// </summary>
    Guid? CreatedBy { get; set; }

    /// <summary>
    /// The ID of the user who last modified this entity.
    /// </summary>
    Guid? ModifiedBy { get; set; }
}
