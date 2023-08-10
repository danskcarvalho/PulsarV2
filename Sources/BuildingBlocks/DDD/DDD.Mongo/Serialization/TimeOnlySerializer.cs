namespace Pulsar.BuildingBlocks.DDD.Mongo.Serialization;

public class TimeOnlySerializer : StructSerializerBase<TimeOnly>
{
    TimeSpanSerializer InnerSerializer = new TimeSpanSerializer();

    public override TimeOnly Deserialize(
        BsonDeserializationContext context,
        BsonDeserializationArgs args)
    {
        var timeSpan = InnerSerializer.Deserialize(context, args);
        return TimeOnly.FromTimeSpan(timeSpan);
    }

    public override void Serialize(
        BsonSerializationContext context,
        BsonSerializationArgs args,
        TimeOnly value) =>
        InnerSerializer.Serialize(
            context,
            args,
            value.ToTimeSpan());
}
