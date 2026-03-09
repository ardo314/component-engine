using System.Numerics;
using Engine.Core;
using Engine.Math;

namespace Modules.InMemoryPose;

public partial class InMemoryPose : ComponentBase<IPose>
{
    private readonly Dictionary<EntityId, Pose> _poses = new();

    public Task OnAddAsync(Entity entity, Pose initialData, CancellationToken ct = default)
    {
        _poses[entity.Id] = initialData;
        return Task.CompletedTask;
    }

    public Task OnRemoveAsync(Entity entity, CancellationToken ct = default)
    {
        _poses.Remove(entity.Id);
        RaiseRemoved(entity);
        return Task.CompletedTask;
    }

    public Task<Pose> GetAsync(Entity entity, CancellationToken ct = default)
    {
        if (!_poses.TryGetValue(entity.Id, out var data))
        {
            data = new Pose { Position = Vector3.Zero, Rotation = Quaternion.Identity };
            _poses[entity.Id] = data;
        }
        return Task.FromResult(data);
    }

    public Task SetAsync(Entity entity, Pose data, CancellationToken ct = default)
    {
        _poses[entity.Id] = data;
        RaiseUpdated(entity, data);
        return Task.CompletedTask;
    }
}
