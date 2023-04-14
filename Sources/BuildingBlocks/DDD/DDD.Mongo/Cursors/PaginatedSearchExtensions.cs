using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulsar.BuildingBlocks.DDD.Mongo.Cursors
{
    public static class PaginatedSearchExtensions
    {

        public static MongoDbCollectionWithCursor<TElement> Paginated<TElement>(this IMongoCollection<TElement> collection, int limit, string? cursor, dynamic filter) where TElement : class
        {
            return new MongoDbCollectionWithCursor<TElement> { Collection = collection, Limit = limit, Cursor = cursor, Filter = filter };
        }
    }
}
