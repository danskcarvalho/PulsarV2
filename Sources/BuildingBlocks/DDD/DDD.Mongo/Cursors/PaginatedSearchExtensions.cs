using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulsar.BuildingBlocks.DDD.Mongo.Cursors
{
    public static class PaginatedSearchExtensions
    {

        public static MongoDbCollectionWithCursor<TElement, TFilter> Paginated<TElement, TFilter>(this IMongoCollection<TElement> collection, IPageCursor<TElement, TFilter> cursor) 
            where TElement : class
        {
            return new MongoDbCollectionWithCursor<TElement, TFilter> { Collection = collection, Cursor = cursor };
        }
    }
}
