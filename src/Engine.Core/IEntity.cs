namespace Engine.Core;

/// <summary>
/// Represents a read-only view of an entity and its components.
/// </summary>
public interface IEntity
{
    /// <summary>
    /// The unique identifier of this entity.
    /// </summary>
    EntityId Id { get; }

    /// <summary>
    /// Adds a component of type <typeparamref name="T"/> to this entity.
    /// </summary>
    Task AddComponentAsync<T>(CancellationToken ct = default)
        where T : IComponent;

    /// <summary>
    /// Removes the component of type <typeparamref name="T"/> from this entity.
    /// </summary>
    Task RemoveComponentAsync<T>(CancellationToken ct = default)
        where T : IComponent;

    /// <summary>
    /// Gets the component of type <typeparamref name="T"/> from this entity.
    /// </summary>
    Task<T> GetComponentAsync<T>(CancellationToken ct = default)
        where T : IComponent;

    /// <summary>
    /// Returns true if the entity has a component of type <typeparamref name="T"/>.
    /// </summary>
    Task<bool> HasComponentAsync<T>(CancellationToken ct = default)
        where T : IComponent;
}
