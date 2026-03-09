using Engine.Core;
using Engine.Hierarchy;

namespace Modules.InMemoryParent;

public partial class InMemoryParent : ComponentBase<IParent>
{
    private readonly Dictionary<EntityId, Parent> _parents = new();

    public Task OnAddAsync(Entity entity, Parent initialData, CancellationToken ct = default)
    {
        _parents[entity.Id] = initialData;
        return Task.CompletedTask;
    }

    public Task OnRemoveAsync(Entity entity, CancellationToken ct = default)
    {
        _parents.Remove(entity.Id);
        RaiseRemoved(entity);
        return Task.CompletedTask;
    }

    public Task<Parent> GetAsync(Entity entity, CancellationToken ct = default)
    {
        if (!_parents.TryGetValue(entity.Id, out var data))
        {
            data = new Parent { ParentId = default };
            _parents[entity.Id] = data;
        }
        return Task.FromResult(data);
    }

    public Task SetAsync(Entity entity, Parent data, CancellationToken ct = default)
    {
        _parents[entity.Id] = data;
        RaiseUpdated(entity, data);
        return Task.CompletedTask;
    }
}
