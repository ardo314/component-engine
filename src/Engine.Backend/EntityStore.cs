using System.Collections.Concurrent;
using Engine.Core;

namespace Engine.Backend;

/// <summary>
/// In-memory storage for entities and their components.
/// </summary>
public sealed class EntityStore
{
    private readonly ConcurrentDictionary<EntityId, EntityRecord> _entities = new();

    public EntityRecord Create()
    {
        var id = EntityId.New();
        var record = new EntityRecord(id);
        if (!_entities.TryAdd(id, record))
        {
            throw new InvalidOperationException($"Entity {id} already exists.");
        }

        return record;
    }

    public EntityRecord Get(EntityId id)
    {
        if (!_entities.TryGetValue(id, out var record))
        {
            throw new KeyNotFoundException($"Entity {id} not found.");
        }

        return record;
    }

    public bool Remove(EntityId id) => _entities.TryRemove(id, out _);

    public bool Exists(EntityId id) => _entities.ContainsKey(id);
}
