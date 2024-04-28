using MongoDB.Bson.Serialization.Attributes;
using Pulsar.BuildingBlocks.DDD.Abstractions;
using System.ComponentModel;

namespace Pulsar.BuildingBlocks.DDD;

public class AggregateComponent : IAggregateComponent
{
    [BsonIgnore]
    private bool _isInitializing = false;

    public AggregateComponent() { }

    public bool IsInitializing => _isInitializing;

    void ISupportInitialize.BeginInit()
    {
        _isInitializing = true;
        OnBeginInit();
    }

    void ISupportInitialize.EndInit()
    {
        _isInitializing = false;
        OnEndInit();
    }

    protected virtual void OnBeginInit() { }
    protected virtual void OnEndInit() { }
}