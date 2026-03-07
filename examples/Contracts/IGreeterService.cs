using Engine.Core;

namespace Examples.Contracts;

/// <summary>
/// Sample service contract for a greeter service.
/// The source generator will produce:
///   - GreeterServiceClient (client proxy implementing this interface)
///   - GreeterServiceServerBase (abstract server stub)
///   - Request/Reply DTOs for each method
/// </summary>
[Generate]
public interface IGreeterService
{
    /// <summary>
    /// Sends a greeting and receives a reply (request-reply pattern).
    /// </summary>
    Task<string> SayHello(string name);

    /// <summary>
    /// Notifies the server that someone joined (publish-subscribe / fire-and-forget pattern).
    /// </summary>
    Task NotifyJoined(string name);
}
