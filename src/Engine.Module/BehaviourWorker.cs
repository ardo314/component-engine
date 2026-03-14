using Engine.Core;

namespace Engine.Module;

public abstract class BehaviourWorker<T>
    where T : IBehaviour
{
    public virtual async Task OnAddedAsync(T behaviour, CancellationToken ct = default)
    {
        // Default implementation for when a behaviour is added to an entity
    }

    public virtual async Task OnRemovedAsync(T behaviour, CancellationToken ct = default)
    {
        // Default implementation for when a behaviour is removed from an entity
    }
}
