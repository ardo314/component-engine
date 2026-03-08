namespace Engine.Core;

/// <summary>
/// Represents the world — the top-level container that manages entity lifecycle.
/// </summary>
public interface IWorld
{
    /// <summary>
    /// Creates a new entity in the world.
    /// </summary>
    Task<Entity> CreateEntityAsync(CancellationToken ct = default);

    /// <summary>
    /// Destroys an entity and all of its components.
    /// </summary>
    Task DestroyEntityAsync(EntityId id, CancellationToken ct = default);

    /// <summary>
    /// Destroys an entity and all of its components.
    /// </summary>
    Task DestroyEntityAsync(Entity entity, CancellationToken ct = default);

    /// <summary>
    /// Gets an entity by its identifier.
    /// </summary>
    Task<Entity> GetEntityAsync(EntityId id, CancellationToken ct = default);
}
