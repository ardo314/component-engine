using System.Numerics;
using Engine.Core;
using Engine.Math;

namespace Modules.DatabasePose;

/// <summary>
/// Behaviour contract for database operations — referenced by DatabasePose.
/// </summary>
public interface IDatabase : IBehaviour
{
    Task CreateRecordAsync(Entity entity, Pose data, CancellationToken ct = default);
    Task DeleteRecordAsync(Entity entity, CancellationToken ct = default);
}

public partial class DatabasePose : ComponentBase<IPose>
{
    public async Task OnAddAsync(Entity entity, Pose initialData, CancellationToken ct = default)
    {
        var database = await entity.GetBehaviourAsync<IDatabase>();
        await database.CreateRecordAsync(entity, initialData, ct);
        Console.WriteLine($"Entity {entity} added to DatabasePose component.");
    }

    public async Task OnRemoveAsync(Entity entity, CancellationToken ct = default)
    {
        var database = await entity.GetBehaviourAsync<IDatabase>();
        await database.DeleteRecordAsync(entity, ct);
        RaiseRemoved(entity);
        Console.WriteLine($"Entity {entity} removed from DatabasePose component.");
    }

    public Task<Pose> GetAsync(Entity entity, CancellationToken ct = default)
    {
        var data = new Pose
        {
            Position = new Vector3(1, 2, 3),
            Rotation = Quaternion.CreateFromYawPitchRoll(0.1f, 0.2f, 0.3f),
        };
        return Task.FromResult(data);
    }

    public Task SetAsync(Entity entity, Pose data, CancellationToken ct = default)
    {
        Console.WriteLine(
            $"Setting pose for entity {entity} to position {data.Position} and rotation {data.Rotation}"
        );
        RaiseUpdated(entity, data);
        return Task.CompletedTask;
    }
}
