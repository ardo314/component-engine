namespace Engine.Core;

/// <summary>
/// Provided by the runtime to extensions during registration.
/// Extensions use this to declare their components and behaviours.
/// </summary>
public interface IExtensionRegistrar
{
    /// <summary>
    /// Registers a component type.
    /// </summary>
    void AddComponent<T>()
        where T : IComponent;

    /// <summary>
    /// Registers a behaviour type with its implementation.
    /// </summary>
    void AddBehaviour<TContract, TImplementation>()
        where TContract : IBehaviour
        where TImplementation : TContract;
}
