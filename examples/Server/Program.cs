using Engine.Runtime;
using Examples.Server;
using NATS.Client.Core;

var opts = NatsOpts.Default with { SerializerRegistry = MessagePackNatsSerializerRegistry.Default };
await using var connection = new NatsConnection(opts);

Console.WriteLine("Server starting... Press Ctrl+C to stop.");

var server = new GreeterService();

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

try
{
    await server.StartAsync(connection, cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("Server stopped.");
}
