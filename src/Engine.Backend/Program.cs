using Engine.Backend;
using NATS.Client.Core;

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

await using var nats = new NatsConnection();
await nats.ConnectAsync();

var repo = new EntityRepository();

await using var entity = new EntityService(nats, repo, cts.Token);
await entity.StartAsync();

Console.WriteLine("Engine.Backend running – press Ctrl+C to stop.");

try
{
    await Task.Delay(Timeout.Infinite, cts.Token);
}
catch (OperationCanceledException)
{
    // graceful shutdown
}
