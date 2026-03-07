using System.Buffers;
using MessagePack;
using MessagePack.Resolvers;
using NATS.Client.Core;

namespace Engine.Runtime;

/// <summary>
/// A NATS serializer registry that uses MessagePack with the contractless resolver.
/// This allows any plain C# class with public properties to be serialized without attributes.
/// </summary>
public sealed class MessagePackNatsSerializerRegistry : INatsSerializerRegistry
{
    /// <summary>
    /// Singleton instance of the registry.
    /// </summary>
    public static readonly MessagePackNatsSerializerRegistry Default = new();

    /// <inheritdoc />
    public INatsSerialize<T> GetSerializer<T>() => MessagePackNatsSerializer<T>.Default;

    /// <inheritdoc />
    public INatsDeserialize<T> GetDeserializer<T>() => MessagePackNatsSerializer<T>.Default;
}

internal sealed class MessagePackNatsSerializer<T> : INatsSerializer<T>
{
    public static readonly MessagePackNatsSerializer<T> Default = new();

    private static readonly MessagePackSerializerOptions Options =
        MessagePackSerializerOptions.Standard.WithResolver(ContractlessStandardResolver.Instance);

    public void Serialize(IBufferWriter<byte> bufferWriter, T value)
    {
        MessagePackSerializer.Serialize(bufferWriter, value, Options);
    }

    public T Deserialize(in ReadOnlySequence<byte> buffer)
    {
        return MessagePackSerializer.Deserialize<T>(buffer, Options);
    }

    public INatsSerializer<T> CombineWith(INatsSerializer<T> next)
    {
        return this;
    }
}
