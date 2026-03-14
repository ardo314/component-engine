using System.Collections.Concurrent;
using Engine.Core;
using NATS.Client.Core;
using NATS.Client.Services;

namespace Engine.Backend;

/// <summary>
/// Tracks which behaviours are attached to each entity and exposes management
/// operations as a NATS service.
///
/// NATS subjects (under the "entity" service group):
///   entity.add-behaviour    – request "entityId:behaviourName", replies "ok" or error.
///   entity.remove-behaviour – request "entityId:behaviourName", replies "ok" or error.
///   entity.has-behaviour    – request "entityId:behaviourName", replies "true" / "false".
///   entity.list-behaviours  – request EntityId (Guid string), replies comma-separated names.
/// </summary>
public sealed class EntityService : IAsyncDisposable
{
    private readonly ConcurrentDictionary<
        EntityId,
        ConcurrentDictionary<string, byte>
    > _behaviours = new();
    private readonly WorldService _world;
    private readonly NatsSvcServer _svc;

    public EntityService(INatsConnection nats, WorldService world, CancellationToken ct)
    {
        _world = world;
        _svc = new NatsSvcServer(nats, new NatsSvcConfig("entity", "1.0.0"), ct);
    }

    /// <summary>
    /// Registers all NATS service endpoints and begins listening for requests.
    /// </summary>
    public async Task StartAsync()
    {
        var grp = await _svc.AddGroupAsync("entity");

        await grp.AddEndpointAsync<string>(name: "add-behaviour", handler: HandleAddBehaviourAsync);
        await grp.AddEndpointAsync<string>(
            name: "remove-behaviour",
            handler: HandleRemoveBehaviourAsync
        );
        await grp.AddEndpointAsync<string>(name: "has-behaviour", handler: HandleHasBehaviourAsync);
        await grp.AddEndpointAsync<string>(
            name: "list-behaviours",
            handler: HandleListBehavioursAsync
        );
    }

    private async ValueTask HandleAddBehaviourAsync(NatsSvcMsg<string> msg)
    {
        if (!TryParseRequest(msg.Data, out var entityId, out var behaviourName))
        {
            await msg.ReplyErrorAsync(400, "Expected format: entityId:behaviourName");
            return;
        }

        if (!_world.EntityExists(entityId))
        {
            await msg.ReplyErrorAsync(404, "Entity not found");
            return;
        }

        var set = _behaviours.GetOrAdd(entityId, _ => new ConcurrentDictionary<string, byte>());

        if (!set.TryAdd(behaviourName, 0))
        {
            await msg.ReplyErrorAsync(409, "Behaviour already added");
            return;
        }

        await msg.ReplyAsync("ok");
    }

    private async ValueTask HandleRemoveBehaviourAsync(NatsSvcMsg<string> msg)
    {
        if (!TryParseRequest(msg.Data, out var entityId, out var behaviourName))
        {
            await msg.ReplyErrorAsync(400, "Expected format: entityId:behaviourName");
            return;
        }

        if (!_world.EntityExists(entityId))
        {
            await msg.ReplyErrorAsync(404, "Entity not found");
            return;
        }

        if (!_behaviours.TryGetValue(entityId, out var set) || !set.TryRemove(behaviourName, out _))
        {
            await msg.ReplyErrorAsync(404, "Behaviour not found on entity");
            return;
        }

        await msg.ReplyAsync("ok");
    }

    private async ValueTask HandleHasBehaviourAsync(NatsSvcMsg<string> msg)
    {
        if (!TryParseRequest(msg.Data, out var entityId, out var behaviourName))
        {
            await msg.ReplyErrorAsync(400, "Expected format: entityId:behaviourName");
            return;
        }

        if (!_world.EntityExists(entityId))
        {
            await msg.ReplyErrorAsync(404, "Entity not found");
            return;
        }

        var has = _behaviours.TryGetValue(entityId, out var set) && set.ContainsKey(behaviourName);
        await msg.ReplyAsync(has ? "true" : "false");
    }

    private async ValueTask HandleListBehavioursAsync(NatsSvcMsg<string> msg)
    {
        if (!Guid.TryParse(msg.Data, out var guid))
        {
            await msg.ReplyErrorAsync(400, "Invalid EntityId format");
            return;
        }

        var entityId = new EntityId(guid);

        if (!_world.EntityExists(entityId))
        {
            await msg.ReplyErrorAsync(404, "Entity not found");
            return;
        }

        if (_behaviours.TryGetValue(entityId, out var set))
        {
            await msg.ReplyAsync(string.Join(",", set.Keys));
        }
        else
        {
            await msg.ReplyAsync(string.Empty);
        }
    }

    private static bool TryParseRequest(
        string? data,
        out EntityId entityId,
        out string behaviourName
    )
    {
        entityId = default;
        behaviourName = string.Empty;

        if (string.IsNullOrEmpty(data))
            return false;

        var sep = data.IndexOf(':');
        if (sep < 0)
            return false;

        if (!Guid.TryParse(data.AsSpan(0, sep), out var guid))
            return false;

        behaviourName = data[(sep + 1)..];
        if (string.IsNullOrWhiteSpace(behaviourName))
            return false;

        entityId = new EntityId(guid);
        return true;
    }

    public async ValueTask DisposeAsync()
    {
        await _svc.DisposeAsync();
    }
}
