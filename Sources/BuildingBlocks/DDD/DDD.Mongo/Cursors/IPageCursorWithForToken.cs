namespace Pulsar.BuildingBlocks.DDD.Mongo.Cursors;

public interface IPageCursorWithForToken<TElement, TFilter> where TElement : class
{
    IPageCursor<TElement, TFilter> ForToken(string? token);
}
