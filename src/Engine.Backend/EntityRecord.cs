using System.Collections.Concurrent;
using Engine.Core;

namespace Engine.Backend;

/// <summary>
/// Holds an entity's identity and its attached components.
/// </summary>
public sealed class EntityRecord
{
    private readonly ConcurrentDictionary<Type, IComponent> _components = new();

    public EntityRecord(EntityId id)
    {
        Id = id;
    }

    public EntityId Id { get; }

    public void AddComponent<T>(T component)
        where T : IComponent
    {
        if (!_components.TryAdd(typeof(T), component))
        {
            throw new InvalidOperationException(
                $"Entity {Id} already has component {typeof(T).Name}."
            );
        }
    }

    public void RemoveComponent<T>()
        where T : IComponent
    {
        if (!_components.TryRemove(typeof(T), out _))
        {
            throw new KeyNotFoundException(
                $"Entity {Id} does not have component {typeof(T).Name}."
            );
        }
    }

    public T GetComponent<T>()
        where T : IComponent
    {
        if (!_components.TryGetValue(typeof(T), out var component))
        {
            throw new KeyNotFoundException(
                $"Entity {Id} does not have component {typeof(T).Name}."
            );
        }

        return (T)component;
    }

    public bool HasComponent<T>()
        where T : IComponent => _components.ContainsKey(typeof(T));
}
