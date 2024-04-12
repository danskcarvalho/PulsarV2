namespace Pulsar.BuildingBlocks.DDD.Mongo.Cursors;

public interface IPageCursor<TElement, TFilter> where TElement : class
{
    string? Next(TElement last);

    string? Token { get; }

    bool TokenIsNotNull { get; }

    (string Name, object? Value) SortField1 { get; }

    (string Name, object? Value)? SortField2 { get; }

    bool HasSortField2 { get; }

    TFilter? Filter { get; }

    int Limit { get; }

    IPageCursorWithForToken<TElement, TAnotherFilter> ForFilter<TAnotherFilter>(TAnotherFilter? filter);

    string? NextFromProjection(object last);
}
