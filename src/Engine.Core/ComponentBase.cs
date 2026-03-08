namespace Engine.Core;

/// <summary>
/// Abstract base class for component implementations.
/// Derive from this class using the component contract interface as the type parameter
/// (e.g., <c>class MyPose : ComponentBase&lt;IPose&gt;</c>).
/// <para>
/// The source generator detects subclasses, resolves the data type from the contract,
/// and emits a partial class that:
/// <list type="bullet">
///   <item>Adds the contract interface (e.g., <c>: IPose</c>)</item>
///   <item>Implements <see cref="IComponent{TData}.DataUpdated"/> and <see cref="IComponent{TData}.DataRemoved"/> events</item>
///   <item>Provides <c>RaiseUpdated</c> and <c>RaiseRemoved</c> helper methods</item>
/// </list>
/// </para>
/// <para>
/// Mark your subclass <c>partial</c> so the generator can extend it.
/// Call <c>RaiseUpdated(entity, data)</c> in your <c>SetAsync</c> implementation
/// and <c>RaiseRemoved(entity)</c> in your <c>OnRemoveAsync</c> implementation.
/// </para>
/// </summary>
/// <typeparam name="TContract">
/// The component contract interface that extends <see cref="IComponent{TData}"/>.
/// </typeparam>
public abstract class ComponentBase<TContract>
    where TContract : IComponent { }
