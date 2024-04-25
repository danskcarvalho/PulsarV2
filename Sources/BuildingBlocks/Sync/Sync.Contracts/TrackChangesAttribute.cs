namespace Pulsar.BuildingBlocks.Sync.Contracts;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public class TrackChangesAttribute : Attribute
{
    public string? CollectionName { get; init; }
    public Type? ShadowType { get; init; }
}
