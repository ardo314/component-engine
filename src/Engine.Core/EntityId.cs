namespace Engine.Core;

/// <summary>
/// Uniquely identifies an Entity within the world.
/// </summary>
public readonly record struct EntityId(Guid Value)
{
    public static EntityId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
