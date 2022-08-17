using MongoDB.Bson;

namespace Pulsar.BuildingBlocks.DDD;

public static class Extensions
{
    public static async Task CheckModified(this Task<long> tl, long expected = 1)
    {
        var tl2 = await tl;
        if (tl2 != expected)
            throw new VersionConcurrencyException($"modified ({tl2}) does not match expected ({expected})");
    }

    public static ObjectId ToObjectId(this string str)
    {
        return ObjectId.Parse(str);
    }
}
