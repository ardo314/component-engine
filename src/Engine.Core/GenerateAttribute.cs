using System;

namespace Engine.Core;

/// <summary>
/// Marks an interface as a service contract for the Engine source generator.
/// The generator will produce a client proxy, a server stub, and message DTOs
/// for NATS-based communication.
/// </summary>
[AttributeUsage(AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
public sealed class GenerateAttribute : Attribute
{
    /// <summary>
    /// Optional override for the service name.
    /// If not set, the service name is derived from the interface name (stripping the leading "I").
    /// </summary>
    public string? ServiceName { get; set; }

    /// <summary>
    /// Optional override for the NATS subject prefix.
    /// If not set, the namespace of the interface is used.
    /// </summary>
    public string? SubjectPrefix { get; set; }
}
