namespace Engine.Module;

using Engine.Core;

public class Entity
{
    public EntityId Id { get; }

    public Entity(EntityId id)
    {
        Id = id;
    }

    public async Task AddBehaviourAsync<TBehaviour>(
        TBehaviour behaviour,
        CancellationToken ct = default
    )
        where TBehaviour : IBehaviour
    {
        // Implementation to add the behaviour to the entity
    }

    public async Task RemoveBehaviourAsync<TBehaviour>(CancellationToken ct = default)
        where TBehaviour : IBehaviour
    {
        // Implementation to remove the behaviour from the entity
    }

    public async Task<bool> HasBehaviourAsync<TBehaviour>(CancellationToken ct = default)
        where TBehaviour : IBehaviour
    {
        // Implementation to check if the entity has the behaviour
        return false;
    }

    public async Task<TBehaviour> GetBehaviourAsync<TBehaviour>(CancellationToken ct = default)
        where TBehaviour : IBehaviour
    {
        // Implementation to retrieve the behaviour from the entity
        return default!;
    }
}
