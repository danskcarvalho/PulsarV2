namespace Pulsar.BuildingBlocks.Migrations;

public static class OtherExtensions
{
    public static async Task<bool> CollectionExists(this IMongoDatabase db, string collectionName)
    {
        var filter = new BsonDocument("name", collectionName);
        var collections = await db.ListCollectionsAsync(new ListCollectionsOptions { Filter = filter });
        return await collections.AnyAsync();
    }
}
