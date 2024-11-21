using MongoDB.Bson;

namespace Pulsar.BuildingBlocks.DDD;

public static class Extensions
{
    public static async Task<long> CheckModified(this Task<long> tl, long? expected = null)
    {
        var tl2 = await tl;
        if (expected == null && tl2 == 0)
            throw new VersionConcurrencyException($"no documents modified");
        if (expected != null && tl2 != expected)
            throw new VersionConcurrencyException($"modified ({tl}) does not match expected ({expected})");

        return tl2;
    }
    
    public static long CheckModified(this long tl, long? expected = null)
    {
        if (expected == null && tl == 0)
            throw new VersionConcurrencyException($"no documents modified");
        if (expected != null && tl != expected)
            throw new VersionConcurrencyException($"modified ({tl}) does not match expected ({expected})");

        return tl;
    }

    public static ObjectId ToObjectId(this string str)
    {
        return ObjectId.Parse(str);
    }
}
