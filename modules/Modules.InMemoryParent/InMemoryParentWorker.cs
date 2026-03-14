using Engine.Core;
using Engine.Module;

namespace Modules.InMemoryParent;

public partial class InMemoryParentWorker : BehaviourWorker<IParent>
{
    public Task InitDataAsync(EntityId data, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<EntityId> GetDataAsync(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task SetDataAsync(EntityId data, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}
