namespace Pulsar.BuildingBlocks.DDD.Mongo.Cursors;

public class PageCursorWithForToken<TElement, TProjection, TFilter, TSort1, TSort2> : PageCursor<TElement, TProjection, TFilter, TSort1, TSort2>, IPageCursorWithForToken<TElement, TFilter> where TElement : class where TProjection : class
{
    public PageCursor<TElement, TProjection, TFilter, TSort1, TSort2> ForToken(string? token)
    {
        if (token is not null)
        {
            var serialized = token.FromBase64Json<SerializedCursor<TFilter, TSort1, TSort2>>();
            if (serialized != null)
            {
                this._filter = serialized.Filter;
                this._lastSort1 = serialized.LastSort1;
                this._lastSort2 = serialized.LastSort2;
                this._token = token;
            }
        }
        return this;
    }

    IPageCursor<TElement, TFilter> IPageCursorWithForToken<TElement, TFilter>.ForToken(string? cursor) => ForToken(cursor);
}
