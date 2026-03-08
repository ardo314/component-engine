using System.Reflection;

namespace Engine.Core;

/// <summary>
/// Entry point for extensions loaded by the runtime.
/// Each extension assembly must contain exactly one implementation of this interface.
/// The runtime discovers it via assembly scanning and calls <see cref="Register"/>
/// to let the extension register its components and behaviours.
/// </summary>
public interface IExtension
{
    /// <summary>
    /// Called by the runtime during startup. The extension should register
    /// its components, behaviours, and any other services.
    /// </summary>
    void Register(IExtensionRegistrar registrar);
}
