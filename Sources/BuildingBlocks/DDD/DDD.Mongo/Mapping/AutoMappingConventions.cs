namespace Pulsar.BuildingBlocks.DDD.Mongo.Mapping;

public static class AutoMappingConventions
{
    public static void Register()
    {
        var pack = new ConventionPack();

        pack.Add(new EnumRepresentationConvention(BsonType.String));

        pack.AddMemberMapConvention("DateShouldBeUtc", c =>
        {
            if (c.MemberType == typeof(DateTime))
                c.SetSerializer(new DateTimeSerializer(DateTimeKind.Utc));
            else if (c.MemberType == typeof(DateTime?))
                c.SetSerializer(new NullableSerializer<DateTime>().WithSerializer(new DateTimeSerializer(DateTimeKind.Utc)));
        });

        ConventionRegistry.Register(
           "PulsarConventions",
           pack,
           t => true);

        BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
    }
}
