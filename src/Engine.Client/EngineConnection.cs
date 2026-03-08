using NATS.Client.Core;

namespace Engine.Client;

/// <summary>
/// Manages the NATS connection for the client.
/// </summary>
public sealed class EngineConnection : IAsyncDisposable
{
    private readonly NatsConnection _connection;

    public EngineConnection(NatsOpts? opts = null)
    {
        _connection = new NatsConnection(opts ?? NatsOpts.Default);
    }

    /// <summary>
    /// The underlying NATS connection.
    /// </summary>
    public INatsConnection Connection => _connection;

    /// <summary>
    /// Connects to the NATS server.
    /// </summary>
    public async Task ConnectAsync(CancellationToken ct = default)
    {
        await _connection.ConnectAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}
