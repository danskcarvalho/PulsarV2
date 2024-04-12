namespace Pulsar.BuildingBlocks.DDD.Mongo.Cursors;

public partial class Paginator
{
    public class PaginatorBuilder
    {
        internal PaginatorBuilder() { }

        public SortedPaginatorBuilder1<TElement, TElement> For<TElement>() where TElement : class
        {
            return new SortedPaginatorBuilder1<TElement, TElement>();
        }

        public SortedPaginatorBuilder1<TElement, TProjection> For<TElement, TProjection>() where TElement : class where TProjection : class
        {
            return new SortedPaginatorBuilder1<TElement, TProjection>();
        }
    }
}
