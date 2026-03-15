using Engine.Client;
using Engine.Core;
using NATS.Client.Core;

Console.WriteLine("Engine.Sandbox starting…");

await using var nats = new NatsConnection();
await nats.ConnectAsync();

var world = new World(nats);

// ── Try things out below this line ──────────────────────────────────────

Console.WriteLine("Connected to NATS. Ready to experiment!");

// Example: create an entity
// var entity = await world.CreateEntityAsync();
// Console.WriteLine($"Created entity {entity.Id}");

Console.WriteLine("Engine.Sandbox finished.");
