namespace Pulsar.BuildingBlocks.DDD.Mongo.Serialization;

public class DateOnlySerializer : StructSerializerBase<DateOnly>
{
    DateTimeSerializer InnerSerializer = DateTimeSerializer.DateOnlyInstance;

    public override DateOnly Deserialize(
        BsonDeserializationContext context,
        BsonDeserializationArgs args)
    {
        var dateTime = InnerSerializer.Deserialize(context, args);
        return DateOnly.FromDateTime(dateTime);
    }

    public override void Serialize(
        BsonSerializationContext context,
        BsonSerializationArgs args,
        DateOnly value) =>
        InnerSerializer.Serialize(
            context,
            args,
            value.ToDateTime(TimeOnly.MinValue));
}
