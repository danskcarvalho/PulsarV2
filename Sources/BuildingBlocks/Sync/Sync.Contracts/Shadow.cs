using MongoDB.Bson;
using Pulsar.BuildingBlocks.DDD;
using System.Reflection;

namespace Pulsar.BuildingBlocks.Sync.Contracts;

public class Shadow : AggregateRoot, IShadow
{
    public Shadow()
    {
    }

    public Shadow(ObjectId id) : base(id)
    {
    }

    public Shadow(ObjectId id, DateTime timeStamp) : base(id)
    {
        this.TimeStamp = timeStamp;
    }

    public DateTime TimeStamp { get; set; }

    public static string GetCollectionName<T>()
    {
        var attr = typeof(T).GetCustomAttribute<ShadowAttribute>();
        if (attr == null)
        {
            throw new InvalidOperationException($"no ShadowAttribute on type {typeof(T).FullName}");
        }

        return $"_{ValidId(attr.Name)}_Shadow";
    }

    private static string ValidId(string name)
    {
        return new string(name.Where(c => char.IsLetterOrDigit(c)).ToArray());
    }
}
