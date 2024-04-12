namespace Pulsar.BuildingBlocks.DDD.Mongo.Cursors;

class SerializedCursor<TFilter, TSort1, TSort2>
{
    public TFilter? Filter { get; set; }
    public TSort1? LastSort1 { get; set; }
    public TSort2? LastSort2 { get; set; }
}
