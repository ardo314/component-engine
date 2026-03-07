using Examples.Contracts;

namespace Examples.Server;

/// <summary>
/// Concrete implementation of the generated GreeterServiceServerBase.
/// Handles incoming NATS requests for the IGreeterService contract.
/// </summary>
public sealed class GreeterService : GreeterServiceServerBase
{
    public override Task<string> SayHello(string name, CancellationToken cancellationToken)
    {
        Console.WriteLine($"[Server] SayHello received: {name}");
        return Task.FromResult($"Hello, {name}!");
    }

    public override Task NotifyJoined(string name, CancellationToken cancellationToken)
    {
        Console.WriteLine($"[Server] {name} has joined!");
        return Task.CompletedTask;
    }
}
