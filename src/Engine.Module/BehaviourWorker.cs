using Engine.Core;

namespace Engine.Module;

public abstract class BehaviourWorker<T>
    where T : IBehaviour
{
    public virtual Task OnAddedAsync(T behaviour, CancellationToken ct = default) =>
        Task.CompletedTask;

    public virtual Task OnRemovedAsync(T behaviour, CancellationToken ct = default) =>
        Task.CompletedTask;
}
