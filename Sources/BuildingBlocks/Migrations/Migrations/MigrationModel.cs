namespace Pulsar.BuildingBlocks.Migrations;

class MigrationModel
{
    [BsonId]
    public ObjectId Id { get; set; }
    public long Version { get; set; }
    public string? Name { get; set; }
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedOn { get; set; }
}
