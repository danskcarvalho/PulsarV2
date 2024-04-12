namespace Pulsar.BuildingBlocks.DDD.Mongo.Cursors;

public class PageCursor<TElement, TProjection, TFilter, TSort1, TSort2> : IPageCursor<TElement, TFilter> where TElement : class where TProjection : class
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    internal string _sort1;
    internal string? _sort2;
    internal Func<TElement, TSort1> _sort1Fn;
    internal Func<TElement, TSort2>? _sort2Fn;
    internal TFilter? _filter;
    internal int _limit;
    protected TSort1? _lastSort1;
    protected TSort2? _lastSort2;
    internal Func<TProjection, TSort1>? _proj1Fn;
    internal Func<TProjection, TSort2>? _proj2Fn;
    protected string? _token = null;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    private string? ToToken()
    {
        if (this._lastSort1 == null && this._lastSort2 == null)
            return null;

        var serialized = new SerializedCursor<TFilter, TSort1, TSort2>()
        {
            Filter = _filter,
            LastSort1 = _lastSort1,
            LastSort2 = _lastSort2
        };
        return serialized.ToBase64Json();
    }

    public PageCursorWithForToken<TElement, TProjection, TAnotherFilter, TSort1, TSort2> ForFilter<TAnotherFilter>(TAnotherFilter? filter)
    {
        if (this._filter != null && typeof(TAnotherFilter) != this._filter.GetType())
            throw new InvalidOperationException("ForFilter must be called before ForToken");

        return new PageCursorWithForToken<TElement, TProjection, TAnotherFilter, TSort1, TSort2>
        {
            _filter = (TAnotherFilter?)(object?)this._filter ?? filter,
            _limit = _limit,
            _sort1 = _sort1,
            _sort1Fn = _sort1Fn,
            _sort2 = _sort2,
            _sort2Fn = _sort2Fn,
            _proj1Fn = _proj1Fn,
            _proj2Fn = _proj2Fn,
            _lastSort1 = this._lastSort1,
            _lastSort2 = this._lastSort2
        };
    }

    internal string? Next(TElement last)
    {
        var next = new PageCursor<TElement, TProjection, TFilter, TSort1, TSort2>
        {
            _sort1 = _sort1,
            _sort2 = _sort2,
            _sort1Fn = _sort1Fn,
            _sort2Fn = _sort2Fn,
            _limit = this._limit,
            _filter = this._filter,
            _proj1Fn = _proj1Fn,
            _proj2Fn = _proj2Fn,
            _lastSort1 = _sort1Fn(last),
            _lastSort2 = _sort2Fn != null ? _sort2Fn(last) : default(TSort2)
        };
        return next.ToToken();
    }

    internal string? NextFromProjection(object last)
    {
        var next = new PageCursor<TElement, TProjection, TFilter, TSort1, TSort2>
        {
            _sort1 = _sort1,
            _sort2 = _sort2,
            _sort1Fn = _sort1Fn,
            _sort2Fn = _sort2Fn,
            _limit = this._limit,
            _filter = this._filter,
            _proj1Fn = _proj1Fn,
            _proj2Fn = _proj2Fn,
            _lastSort1 = _proj1Fn == null ? throw new InvalidOperationException("no projection specified") : _proj1Fn((TProjection)last),
            _lastSort2 = _proj2Fn != null ? _proj2Fn((TProjection)last) : default(TSort2)
        };
        return next.ToToken();
    }

    string? IPageCursor<TElement, TFilter>.Next(TElement last) => Next(last);

    IPageCursorWithForToken<TElement, TAnotherFilter> IPageCursor<TElement, TFilter>.ForFilter<TAnotherFilter>(TAnotherFilter? filter) where TAnotherFilter : default => ForFilter(filter);

    string? IPageCursor<TElement, TFilter>.NextFromProjection(object last) => NextFromProjection(last);

    internal string? Token => ToToken();

    internal bool TokenIsNotNull => this._token != null;

    internal (string Name, object? Value) SortField1 => (_sort1, _lastSort1);

    internal (string Name, object? Value)? SortField2 => _sort2 is null ? throw new InvalidOperationException("no second field to sort") : (_sort2, _lastSort2);

    internal bool HasSortField2 => _sort2 is not null;

    internal TFilter? Filter => _filter;

    internal int Limit => _limit;

    string? IPageCursor<TElement, TFilter>.Token => Token;

    bool IPageCursor<TElement, TFilter>.TokenIsNotNull => TokenIsNotNull;

    (string Name, object? Value) IPageCursor<TElement, TFilter>.SortField1 => SortField1;

    (string Name, object? Value)? IPageCursor<TElement, TFilter>.SortField2 => SortField2;

    TFilter? IPageCursor<TElement, TFilter>.Filter => Filter;

    int IPageCursor<TElement, TFilter>.Limit => Limit;

    bool IPageCursor<TElement, TFilter>.HasSortField2 => HasSortField2;
}
