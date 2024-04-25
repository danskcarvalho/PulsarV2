using System.Text.Json.Serialization;

namespace Pulsar.BuildingBlocks.Syncing.DDD;

public class EntityChangedProperty
{
    public string PropertyName { get; }
    public string Previous { get; }
    public string Value { get; }

    [JsonConstructor]
    public EntityChangedProperty(string propertyName, string previous, string value)
    {
        PropertyName = propertyName;
        Value = value;
        Previous = previous;
    }
}