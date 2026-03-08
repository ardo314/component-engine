namespace Engine.Core;

/// <summary>
/// Marker interface for all components.
/// A component holds data or state that can be attached to an Entity.
/// </summary>
public interface IComponent
{
    /// <summary>
    /// Called when the component is removed from an entity.
    /// Implementations should clean up any entity-specific state and call <c>RaiseRemoved</c>.
    /// </summary>
    Task OnRemoveAsync(Entity entity, CancellationToken ct = default);
}

/// <summary>
/// A typed component that stores data of type <typeparamref name="TData"/> on entities.
/// Implementations must fire <see cref="DataUpdated"/> and <see cref="DataRemoved"/>
/// from their <see cref="SetAsync"/> and <see cref="IComponent.OnRemoveAsync"/> methods respectively
/// when the operation succeeds.
/// </summary>
/// <typeparam name="TData">The data type this component stores per entity.</typeparam>
public interface IComponent<TData> : IComponent
{
    /// <summary>
    /// Called when the component is added to an entity.
    /// </summary>
    Task OnAddAsync(Entity entity, TData initialData, CancellationToken ct = default);

    /// <summary>
    /// Gets the component data for the given entity.
    /// </summary>
    Task<TData> GetAsync(Entity entity, CancellationToken ct = default);

    /// <summary>
    /// Sets the component data for the given entity.
    /// Implementations should call <c>RaiseUpdated</c> after a successful write.
    /// </summary>
    Task SetAsync(Entity entity, TData data, CancellationToken ct = default);

    /// <summary>
    /// Fired by the implementation when component data is successfully updated for an entity.
    /// </summary>
    event Action<Entity, TData>? DataUpdated;

    /// <summary>
    /// Fired by the implementation when component data is successfully removed for an entity.
    /// </summary>
    event Action<Entity>? DataRemoved;
}
