using System.Numerics;
using Engine.Core;

namespace Example;

public struct Pose
{
    public Vector3 Position { get; init; }
    public Quaternion Rotation { get; init; }
}

public interface IPose : IComponent<Pose> { }

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

/// <summary>
/// Example behaviour contract — referenced by DatabasePose but not implemented here.
/// </summary>
public interface IDatabase : IBehaviour
{
    Task CreateRecordAsync(Entity entity, Pose data, CancellationToken ct = default);
    Task DeleteRecordAsync(Entity entity, CancellationToken ct = default);
}

public class SomeUserPlugin : Plugin
{
    public override async Task OnStartAsync(CancellationToken ct = default)
    {
        var entity = await World.CreateEntityAsync(ct);

        // Adding components — must use concrete type
        var pose = await entity.AddComponentAsync<InMemoryPose, Pose>(
            new Pose { Position = Vector3.Zero, Rotation = Quaternion.Identity },
            ct
        );

        // Getting component data — either interface or concrete type
        var pose2 = await entity.GetComponentAsync<IPose, Pose>(ct);
        var pose3 = await entity.GetComponentAsync<InMemoryPose, Pose>(ct);

        // DatabasePose was never added, so this returns null
        var pose4 = await entity.GetComponentAsync<DatabasePose, Pose>(ct);

        if (pose2 is not null)
        {
            pose2.Updated += (data) =>
            {
                Console.WriteLine($"Entity {entity} pose updated: {data}");
            };
            pose2.Removed += () =>
            {
                Console.WriteLine($"Entity {entity} pose removed");
            };

            var poseData = await pose2.GetAsync(ct);

            await pose2.SetAsync(
                new Pose
                {
                    Position = new Vector3(1, 2, 3),
                    Rotation = Quaternion.CreateFromYawPitchRoll(0.1f, 0.2f, 0.3f),
                },
                ct
            );
        }

        await entity.HasComponentAsync<IPose>(ct); // true
        await entity.HasComponentAsync<InMemoryPose>(ct); // true
        await entity.HasComponentAsync<DatabasePose>(ct); // false

        // Removing components — either interface or concrete type works
        await entity.RemoveComponentAsync<IPose>(ct);

        await World.DestroyEntityAsync(entity, ct);
    }

    public override Task OnStopAsync(CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }
}
