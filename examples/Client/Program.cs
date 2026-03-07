using Engine.Runtime;
using Examples.Contracts;
using NATS.Client.Core;

var opts = NatsOpts.Default with { SerializerRegistry = MessagePackNatsSerializerRegistry.Default };
await using var connection = new NatsConnection(opts);

var client = new GreeterServiceClient(connection);

// Request-reply: sends a request and waits for a response
var reply = await client.SayHello("World");
Console.WriteLine($"Reply: {reply}");

// Publish-subscribe: fire-and-forget notification
await client.NotifyJoined("Alice");
Console.WriteLine("Notification sent!");
