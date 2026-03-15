using System.Collections.Concurrent;
using Engine.Core;

namespace Engine.Backend;

/// <summary>
/// Central in-memory store for entity existence and per-entity behaviour sets.
/// Shared by <see cref="WorldService"/> and <see cref="EntityService"/>.
/// </summary>
public sealed class EntityRepository
{
    private readonly ConcurrentDictionary<EntityId, ConcurrentDictionary<string, byte>> _entities =
        new();

    /// <summary>Creates a new entity and returns its id.</summary>
    public EntityId Create()
    {
        var id = EntityId.New();
        _entities.TryAdd(id, new ConcurrentDictionary<string, byte>());
        return id;
    }

    /// <summary>
    /// Removes an entity and all of its behaviours.
    /// Returns the list of behaviour names that were attached, or <c>null</c> if the entity did not exist.
    /// </summary>
    public ICollection<string>? Destroy(EntityId id)
    {
        if (_entities.TryRemove(id, out var set))
            return set.Keys;

        return null;
    }

    /// <summary>Returns whether the entity exists.</summary>
    public bool Exists(EntityId id) => _entities.ContainsKey(id);

    /// <summary>Returns all known entity ids.</summary>
    public ICollection<EntityId> ListAll() => _entities.Keys;

    /// <summary>Adds a behaviour to an entity. Returns false if already present.</summary>
    public bool AddBehaviour(EntityId id, string behaviourName)
    {
        if (!_entities.TryGetValue(id, out var set))
            return false;

        return set.TryAdd(behaviourName, 0);
    }

    /// <summary>Removes a behaviour from an entity. Returns false if not found.</summary>
    public bool RemoveBehaviour(EntityId id, string behaviourName)
    {
        if (!_entities.TryGetValue(id, out var set))
            return false;

        return set.TryRemove(behaviourName, out _);
    }

    /// <summary>Checks whether an entity has a given behaviour.</summary>
    public bool HasBehaviour(EntityId id, string behaviourName)
    {
        return _entities.TryGetValue(id, out var set) && set.ContainsKey(behaviourName);
    }

    /// <summary>Returns the behaviour names for an entity, or an empty collection if none.</summary>
    public ICollection<string> ListBehaviours(EntityId id)
    {
        if (_entities.TryGetValue(id, out var set))
            return set.Keys;

        return Array.Empty<string>();
    }
}
