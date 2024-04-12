using static Pulsar.BuildingBlocks.DDD.Mongo.Cursors.Paginator;

namespace Pulsar.BuildingBlocks.DDD.Mongo.Cursors;

public abstract class Paginator<TElement> where TElement : class
{
    public abstract IPageCursor<TElement, object> ForLimit(int limit);
}

public partial class Paginator
{
    public static PaginatorBuilder Builder => new PaginatorBuilder();
}
