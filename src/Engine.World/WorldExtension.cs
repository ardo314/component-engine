using Engine.Core;

namespace Engine.World;

/// <summary>
/// Extension entry point for the world module.
/// Registers the core entity/component management types.
/// </summary>
public sealed class WorldExtension : IExtension
{
    public void Register(IExtensionRegistrar registrar)
    {
        // World management is a built-in behaviour set.
        // Component and behaviour registrations will be added here
        // as the world module grows.
    }
}
