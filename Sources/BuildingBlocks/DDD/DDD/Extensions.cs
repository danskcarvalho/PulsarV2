using MongoDB.Bson;

namespace Pulsar.BuildingBlocks.DDD;

public static class Extensions
{
    public static async Task<long> CheckModified(this Task<long> tl, long expected = 1)
    {
        var tl2 = await tl;
        if (tl2 != expected)
            throw new VersionConcurrencyException($"modified ({tl2}) does not match expected ({expected})");

        return tl2;
    }
    
    public static long CheckModified(this long tl, long expected = 1)
    {
        if (tl != expected)
            throw new VersionConcurrencyException($"modified ({tl}) does not match expected ({expected})");

        return tl;
    }

    public static ObjectId ToObjectId(this string str)
    {
        return ObjectId.Parse(str);
    }
}
