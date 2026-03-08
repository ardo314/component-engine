namespace Engine.Core;

/// <summary>
/// Marker interface for all behaviours.
/// A behaviour is logic that operates on components and runs as a remote service.
/// Its interface is defined in Engine.Core; its implementation lives in Engine.Backend;
/// its generated proxy lives in Engine.Client.
/// </summary>
public interface IBehaviour { }
