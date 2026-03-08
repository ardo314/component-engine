namespace Engine.Core;

/// <summary>
/// Non-generic base interface for component handles.
/// Returned by single-type-parameter overloads on <see cref="Entity"/>.
/// </summary>
public interface IComponentHandle
{
    /// <summary>
    /// Fired when the component is removed from the entity.
    /// </summary>
    event Action? Removed;
}

/// <summary>
/// A typed, entity-bound view of a component.
/// Wraps an <see cref="IComponent{TData}"/> instance and a specific <see cref="Entity"/>
/// so callers can use <see cref="GetAsync"/> and <see cref="SetAsync"/> without passing
/// the entity on every call.
/// </summary>
/// <typeparam name="TData">The component data type.</typeparam>
public sealed class ComponentHandle<TData> : IComponentHandle
{
    private readonly Entity _entity;
    private readonly IComponent<TData> _component;

    /// <summary>
    /// Fired when the component data is successfully updated for this entity.
    /// </summary>
    public event Action<TData>? Updated;

    /// <summary>
    /// Fired when the component is removed from this entity.
    /// </summary>
    public event Action? Removed;

    internal ComponentHandle(Entity entity, IComponent<TData> component)
    {
        _entity = entity;
        _component = component;

        // Subscribe to the component's global events and filter by entity.
        _component.DataUpdated += OnDataUpdated;
        _component.DataRemoved += OnDataRemoved;
    }

    /// <summary>
    /// Gets the component data for this entity.
    /// </summary>
    public Task<TData> GetAsync(CancellationToken ct = default) => _component.GetAsync(_entity, ct);

    /// <summary>
    /// Sets the component data for this entity.
    /// </summary>
    public Task SetAsync(TData data, CancellationToken ct = default) =>
        _component.SetAsync(_entity, data, ct);

    private void OnDataUpdated(Entity entity, TData data)
    {
        if (entity.Id == _entity.Id)
            Updated?.Invoke(data);
    }

    private void OnDataRemoved(Entity entity)
    {
        if (entity.Id == _entity.Id)
        {
            Removed?.Invoke();
            Detach();
        }
    }

    /// <summary>
    /// Detaches this handle from the component's events.
    /// Called automatically when the component is removed.
    /// </summary>
    internal void Detach()
    {
        _component.DataUpdated -= OnDataUpdated;
        _component.DataRemoved -= OnDataRemoved;
    }
}
