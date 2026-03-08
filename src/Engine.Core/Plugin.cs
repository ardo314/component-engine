namespace Engine.Core;

/// <summary>
/// Base class for user plugins that run logic against the world.
/// The runtime injects <see cref="World"/> before calling lifecycle methods.
/// </summary>
public abstract class Plugin
{
    private IWorld? _world;

    /// <summary>
    /// The world this plugin operates in. Set by the runtime before <see cref="OnStartAsync"/>.
    /// </summary>
    protected IWorld World =>
        _world
        ?? throw new InvalidOperationException(
            "World has not been set. The runtime must call Initialize before OnStartAsync."
        );

    /// <summary>
    /// Called by the runtime to inject the world before starting the plugin.
    /// Not intended for user code.
    /// </summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public void Initialize(IWorld world) => _world = world;

    /// <summary>
    /// Called by the runtime after the world is ready.
    /// </summary>
    public abstract Task OnStartAsync(CancellationToken ct = default);

    /// <summary>
    /// Called by the runtime during graceful shutdown.
    /// </summary>
    public abstract Task OnStopAsync(CancellationToken ct = default);
}
