using MongoDB.Bson;
using Pulsar.BuildingBlocks.DDD;
using System.Reflection;
using DDD.Contracts;
using Pulsar.BuildingBlocks.DDD.Abstractions;

namespace Pulsar.BuildingBlocks.Sync.Contracts;

public class Shadow<TSelf> : AggregateRootWithContext<TSelf>, IShadow where TSelf : class, IAggregateRoot
{
    public Shadow()
    {
    }

    public Shadow(ObjectId id) : base(id)
    {
    }

    public void InitializeShadowFromRoot(IAggregateRoot root)
    {
        this.Id = root.Id;
        this.Version = root.Version;
    }

    public static string GetCollectionName()
    {
        var attr = typeof(TSelf).GetCustomAttribute<ShadowAttribute>();
        if (attr == null)
        {
            throw new InvalidOperationException($"no ShadowAttribute on type {typeof(TSelf).FullName}");
        }

        return $"_{ValidId(attr.Name)}_Shadow";
    }

    private static string ValidId(string name)
    {
        return new string(name.Where(c => char.IsLetterOrDigit(c)).ToArray());
    }
}
