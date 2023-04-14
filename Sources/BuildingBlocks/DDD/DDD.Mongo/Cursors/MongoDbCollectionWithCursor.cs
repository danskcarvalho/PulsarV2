using Pulsar.BuildingBlocks.Utils;
using Pulsar.BuildingBlocks.Utils.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulsar.BuildingBlocks.DDD.Mongo.Cursors
{
    public record MongoDbCollectionWithCursor<TElement> 
        where TElement : class
    {
        public required IMongoCollection<TElement> Collection { get; init; }
        public required int Limit { get; init; }
        public required string? Cursor { get; init; }
        public required dynamic Filter { get; init; }

        public Task<DataAndNext<TElement>> FindAsync<TCursor>(Func<TCursor, BsonDocument?>? bsonFilter = null)
            where TCursor : class, IPageCursor<TCursor, TElement>
        {
            if (TCursor.HasSortColumn2)
                return FindAsync2(bsonFilter);
            else
                return FindAsync1(bsonFilter);
        }

        public Task<DataAndNext<TProjection>> FindAsync<TCursor, TProjection>(ProjectionDefinition<TElement, TProjection> projection, Func<TCursor, BsonDocument?>? bsonFilter = null)
            where TProjection : class
            where TCursor : class, IPageCursor<TCursor, TProjection>
        {
            if (TCursor.HasSortColumn2)
                return FindAsync2(projection, bsonFilter);
            else
                return FindAsync1(projection, bsonFilter);
        }
        public Task<DataAndNext<TElement>> FindAsyncWithAsyncFilter<TCursor>(Func<TCursor, Task<BsonDocument?>>? bsonFilter = null)
            where TCursor : class, IPageCursor<TCursor, TElement>
        {
            if (TCursor.HasSortColumn2)
                return FindAsync2(bsonFilter);
            else
                return FindAsync1(bsonFilter);
        }

        public Task<DataAndNext<TProjection>> FindAsyncWithAsyncFilter<TCursor, TProjection>(ProjectionDefinition<TElement, TProjection> projection, Func<TCursor, Task<BsonDocument?>>? bsonFilter = null)
            where TProjection : class
            where TCursor : class, IPageCursor<TCursor, TProjection>
        {
            if (TCursor.HasSortColumn2)
                return FindAsync2(projection, bsonFilter);
            else
                return FindAsync1(projection, bsonFilter);
        }

        private async Task<DataAndNext<TElement>> FindAsync2<TCursor>(Func<TCursor, BsonDocument?>? bsonFilter = null)
            where TCursor : class, IPageCursor<TCursor, TElement>
        {
            if (this.Cursor is not null)
            {
                var limit = this.Limit.Limit();
                var cursorObj = this.Cursor.FromBase64Json<TCursor>()!;
                var innerFilter = bsonFilter is not null ? bsonFilter(cursorObj) : new BsonDocument();
                if (innerFilter is null)
                    return new DataAndNext<TElement>(new List<TElement>(), null);

                var filter = BSON.Create(b => b.And(
                    innerFilter,
                    b.Or(new Dictionary<string, object> { [cursorObj.SortColumn1.Name] = b.Gt(cursorObj.SortColumn1.Value) }, 
                        b.And(new Dictionary<string, object> { [cursorObj.SortColumn1.Name] = b.Eq(cursorObj.SortColumn1.Value) }, new Dictionary<string, object> { [cursorObj.SortColumn2!.Value.Name] = b.Gt(cursorObj.SortColumn2!.Value.Value) }))));

                var findOptions = new FindOptions<TElement, TElement>()
                {
                    Sort = Builders<TElement>.Sort.Ascending(cursorObj.SortColumn1.Name).Ascending(cursorObj.SortColumn2!.Value.Name),
                    Limit = limit
                };
                var items = await Collection.FindAsync(filter, findOptions).ToListAsync();
                return new DataAndNext<TElement>(items, items.Count != 0 ? cursorObj.Next(items.Last()).ToBase64Json() : null);
            }
            else
            {
                var limit = this.Limit.Limit();
                var cursorObj = (TCursor)TCursor.FromFilter(this.Filter);
                var innerFilter = bsonFilter is not null ? bsonFilter(cursorObj) : new BsonDocument();
                if (innerFilter is null)
                    return new DataAndNext<TElement>(new List<TElement>(), null);

                var findOptions = new FindOptions<TElement, TElement>()
                {
                    Sort = Builders<TElement>.Sort.Ascending(cursorObj.SortColumn1.Name).Ascending(cursorObj.SortColumn2!.Value.Name),
                    Limit = limit
                };
                var items = await Collection.FindAsync(innerFilter, findOptions).ToListAsync();
                return new DataAndNext<TElement>(items, items.Count != 0 ? cursorObj.Next(items.Last()).ToBase64Json() : null);
            }
        }

        private async Task<DataAndNext<TElement>> FindAsync1<TCursor>(Func<TCursor, BsonDocument?>? bsonFilter = null)
            where TCursor : class, IPageCursor<TCursor, TElement>
        {
            if (this.Cursor is not null)
            {
                var limit = this.Limit.Limit();
                var cursorObj = this.Cursor.FromBase64Json<TCursor>()!;
                var innerFilter = bsonFilter is not null ? bsonFilter(cursorObj) : new BsonDocument();
                if (innerFilter is null)
                    return new DataAndNext<TElement>(new List<TElement>(), null);
                var filter = BSON.Create(b => b.And(
                    innerFilter,
                    new Dictionary<string, object> { [cursorObj.SortColumn1.Name] = b.Gt(cursorObj.SortColumn1.Value) } ));

                var findOptions = new FindOptions<TElement, TElement>()
                {
                    Sort = Builders<TElement>.Sort.Ascending(cursorObj.SortColumn1.Name),
                    Limit = limit
                };
                var items = await Collection.FindAsync(filter, findOptions).ToListAsync();
                return new DataAndNext<TElement>(items, items.Count != 0 ? cursorObj.Next(items.Last()).ToBase64Json() : null);
            }
            else
            {
                var limit = this.Limit.Limit();
                var cursorObj = (TCursor)TCursor.FromFilter(this.Filter);
                var innerFilter = bsonFilter is not null ? bsonFilter(cursorObj) : new BsonDocument();
                if (innerFilter is null)
                    return new DataAndNext<TElement>(new List<TElement>(), null);

                var findOptions = new FindOptions<TElement, TElement>()
                {
                    Sort = Builders<TElement>.Sort.Ascending(cursorObj.SortColumn1.Name),
                    Limit = limit
                };
                var items = await Collection.FindAsync(innerFilter, findOptions).ToListAsync();
                return new DataAndNext<TElement>(items, items.Count != 0 ? cursorObj.Next(items.Last()).ToBase64Json() : null);
            }
        }

        private async Task<DataAndNext<TProjection>> FindAsync2<TCursor, TProjection>(ProjectionDefinition<TElement, TProjection> projection, Func<TCursor, BsonDocument?>? bsonFilter = null)
            where TProjection : class
            where TCursor : class, IPageCursor<TCursor, TProjection>
        {
            if (this.Cursor is not null)
            {
                var limit = this.Limit.Limit();
                var cursorObj = this.Cursor.FromBase64Json<TCursor>()!;
                var innerFilter = bsonFilter is not null ? bsonFilter(cursorObj) : new BsonDocument();
                if (innerFilter is null)
                    return new DataAndNext<TProjection>(new List<TProjection>(), null);
                var filter = BSON.Create(b => b.And(
                    innerFilter,
                    b.Or(new Dictionary<string, object> { [cursorObj.SortColumn1.Name] = b.Gt(cursorObj.SortColumn1.Value) },
                        b.And(new Dictionary<string, object> { [cursorObj.SortColumn1.Name] = b.Eq(cursorObj.SortColumn1.Value) }, new Dictionary<string, object> { [cursorObj.SortColumn2!.Value.Name] = b.Gt(cursorObj.SortColumn2!.Value.Value) }))));

                var findOptions = new FindOptions<TElement, TProjection>()
                {
                    Sort = Builders<TElement>.Sort.Ascending(cursorObj.SortColumn1.Name).Ascending(cursorObj.SortColumn2!.Value.Name),
                    Limit = limit,
                    Projection = projection
                };
                var items = await Collection.FindAsync(filter, findOptions).ToListAsync();
                return new DataAndNext<TProjection>(items, items.Count != 0 ? cursorObj.Next(items.Last()).ToBase64Json() : null);
            }
            else
            {
                var limit = this.Limit.Limit();
                var cursorObj = (TCursor)TCursor.FromFilter(this.Filter);
                var innerFilter = bsonFilter is not null ? bsonFilter(cursorObj) : new BsonDocument();
                if (innerFilter is null)
                    return new DataAndNext<TProjection>(new List<TProjection>(), null);

                var findOptions = new FindOptions<TElement, TProjection>()
                {
                    Sort = Builders<TElement>.Sort.Ascending(cursorObj.SortColumn1.Name).Ascending(cursorObj.SortColumn2!.Value.Name),
                    Limit = limit,
                    Projection = projection
                };
                var items = await Collection.FindAsync(innerFilter, findOptions).ToListAsync();
                return new DataAndNext<TProjection>(items, items.Count != 0 ? cursorObj.Next(items.Last()).ToBase64Json() : null);
            }
        }

        private async Task<DataAndNext<TProjection>> FindAsync1<TCursor, TProjection>(ProjectionDefinition<TElement, TProjection> projection, Func<TCursor, BsonDocument?>? bsonFilter = null)
            where TProjection : class
            where TCursor : class, IPageCursor<TCursor, TProjection>
        {
            if (this.Cursor is not null)
            {
                var limit = this.Limit.Limit();
                var cursorObj = this.Cursor.FromBase64Json<TCursor>()!;
                var innerFilter = bsonFilter is not null ? bsonFilter(cursorObj) : new BsonDocument();
                if (innerFilter is null)
                    return new DataAndNext<TProjection>(new List<TProjection>(), null);
                var filter = BSON.Create(b => b.And(
                    innerFilter,
                    new Dictionary<string, object> { [cursorObj.SortColumn1.Name] = b.Gt(cursorObj.SortColumn1.Value) }));

                var findOptions = new FindOptions<TElement, TProjection>()
                {
                    Sort = Builders<TElement>.Sort.Ascending(cursorObj.SortColumn1.Name),
                    Limit = limit,
                    Projection = projection
                };
                var items = await Collection.FindAsync(filter, findOptions).ToListAsync();
                return new DataAndNext<TProjection>(items, items.Count != 0 ? cursorObj.Next(items.Last()).ToBase64Json() : null);
            }
            else
            {
                var limit = this.Limit.Limit();
                var cursorObj = (TCursor)TCursor.FromFilter(this.Filter);
                var innerFilter = bsonFilter is not null ? bsonFilter(cursorObj) : new BsonDocument();
                if (innerFilter is null)
                    return new DataAndNext<TProjection>(new List<TProjection>(), null);

                var findOptions = new FindOptions<TElement, TProjection>()
                {
                    Sort = Builders<TElement>.Sort.Ascending(cursorObj.SortColumn1.Name),
                    Limit = limit,
                    Projection = projection
                };
                var items = await Collection.FindAsync(innerFilter, findOptions).ToListAsync();
                return new DataAndNext<TProjection>(items, items.Count != 0 ? cursorObj.Next(items.Last()).ToBase64Json() : null);
            }
        }

        private async Task<DataAndNext<TElement>> FindAsync2<TCursor>(Func<TCursor, Task<BsonDocument?>>? bsonFilter = null)
            where TCursor : class, IPageCursor<TCursor, TElement>
        {
            if (this.Cursor is not null)
            {
                var limit = this.Limit.Limit();
                var cursorObj = this.Cursor.FromBase64Json<TCursor>()!;
                var innerFilter = bsonFilter is not null ? await bsonFilter(cursorObj) : new BsonDocument();
                if (innerFilter is null)
                    return new DataAndNext<TElement>(new List<TElement>(), null);
                var filter = BSON.Create(b => b.And(
                    innerFilter,
                    b.Or(new Dictionary<string, object> { [cursorObj.SortColumn1.Name] = b.Gt(cursorObj.SortColumn1.Value) },
                        b.And(new Dictionary<string, object> { [cursorObj.SortColumn1.Name] = b.Eq(cursorObj.SortColumn1.Value) }, new Dictionary<string, object> { [cursorObj.SortColumn2!.Value.Name] = b.Gt(cursorObj.SortColumn2!.Value.Value) }))));

                var findOptions = new FindOptions<TElement, TElement>()
                {
                    Sort = Builders<TElement>.Sort.Ascending(cursorObj.SortColumn1.Name).Ascending(cursorObj.SortColumn2!.Value.Name),
                    Limit = limit
                };
                var items = await Collection.FindAsync(filter, findOptions).ToListAsync();
                return new DataAndNext<TElement>(items, items.Count != 0 ? cursorObj.Next(items.Last()).ToBase64Json() : null);
            }
            else
            {
                var limit = this.Limit.Limit();
                var cursorObj = (TCursor)TCursor.FromFilter(this.Filter);
                var innerFilter = bsonFilter is not null ? await bsonFilter(cursorObj) : new BsonDocument();
                if (innerFilter is null)
                    return new DataAndNext<TElement>(new List<TElement>(), null);

                var findOptions = new FindOptions<TElement, TElement>()
                {
                    Sort = Builders<TElement>.Sort.Ascending(cursorObj.SortColumn1.Name).Ascending(cursorObj.SortColumn2!.Value.Name),
                    Limit = limit
                };
                var items = await Collection.FindAsync(innerFilter, findOptions).ToListAsync();
                return new DataAndNext<TElement>(items, items.Count != 0 ? cursorObj.Next(items.Last()).ToBase64Json() : null);
            }
        }

        private async Task<DataAndNext<TElement>> FindAsync1<TCursor>(Func<TCursor, Task<BsonDocument?>>? bsonFilter = null)
            where TCursor : class, IPageCursor<TCursor, TElement>
        {
            if (this.Cursor is not null)
            {
                var limit = this.Limit.Limit();
                var cursorObj = this.Cursor.FromBase64Json<TCursor>()!;
                var innerFilter = bsonFilter is not null ? await bsonFilter(cursorObj) : new BsonDocument();
                if (innerFilter is null)
                    return new DataAndNext<TElement>(new List<TElement>(), null);
                var filter = BSON.Create(b => b.And(
                    innerFilter,
                    new Dictionary<string, object> { [cursorObj.SortColumn1.Name] = b.Gt(cursorObj.SortColumn1.Value) }));

                var findOptions = new FindOptions<TElement, TElement>()
                {
                    Sort = Builders<TElement>.Sort.Ascending(cursorObj.SortColumn1.Name),
                    Limit = limit
                };
                var items = await Collection.FindAsync(filter, findOptions).ToListAsync();
                return new DataAndNext<TElement>(items, items.Count != 0 ? cursorObj.Next(items.Last()).ToBase64Json() : null);
            }
            else
            {
                var limit = this.Limit.Limit();
                var cursorObj = (TCursor)TCursor.FromFilter(this.Filter);
                var innerFilter = bsonFilter is not null ? await bsonFilter(cursorObj) : new BsonDocument();
                if (innerFilter is null)
                    return new DataAndNext<TElement>(new List<TElement>(), null);

                var findOptions = new FindOptions<TElement, TElement>()
                {
                    Sort = Builders<TElement>.Sort.Ascending(cursorObj.SortColumn1.Name),
                    Limit = limit
                };
                var items = await Collection.FindAsync(innerFilter, findOptions).ToListAsync();
                return new DataAndNext<TElement>(items, items.Count != 0 ? cursorObj.Next(items.Last()).ToBase64Json() : null);
            }
        }

        private async Task<DataAndNext<TProjection>> FindAsync2<TCursor, TProjection>(ProjectionDefinition<TElement, TProjection> projection, Func<TCursor, Task<BsonDocument?>>? bsonFilter = null)
            where TProjection : class
            where TCursor : class, IPageCursor<TCursor, TProjection>
        {
            if (this.Cursor is not null)
            {
                var limit = this.Limit.Limit();
                var cursorObj = this.Cursor.FromBase64Json<TCursor>()!;
                var innerFilter = bsonFilter is not null ? await bsonFilter(cursorObj) : new BsonDocument();
                if (innerFilter is null)
                    return new DataAndNext<TProjection>(new List<TProjection>(), null);
                var filter = BSON.Create(b => b.And(
                    innerFilter,
                    b.Or(new Dictionary<string, object> { [cursorObj.SortColumn1.Name] = b.Gt(cursorObj.SortColumn1.Value) },
                        b.And(new Dictionary<string, object> { [cursorObj.SortColumn1.Name] = b.Eq(cursorObj.SortColumn1.Value) }, new Dictionary<string, object> { [cursorObj.SortColumn2!.Value.Name] = b.Gt(cursorObj.SortColumn2!.Value.Value) }))));

                var findOptions = new FindOptions<TElement, TProjection>()
                {
                    Sort = Builders<TElement>.Sort.Ascending(cursorObj.SortColumn1.Name).Ascending(cursorObj.SortColumn2!.Value.Name),
                    Limit = limit,
                    Projection = projection
                };
                var items = await Collection.FindAsync(filter, findOptions).ToListAsync();
                return new DataAndNext<TProjection>(items, items.Count != 0 ? cursorObj.Next(items.Last()).ToBase64Json() : null);
            }
            else
            {
                var limit = this.Limit.Limit();
                var cursorObj = (TCursor)TCursor.FromFilter(this.Filter);
                var innerFilter = bsonFilter is not null ? await bsonFilter(cursorObj) : new BsonDocument();
                if (innerFilter is null)
                    return new DataAndNext<TProjection>(new List<TProjection>(), null);

                var findOptions = new FindOptions<TElement, TProjection>()
                {
                    Sort = Builders<TElement>.Sort.Ascending(cursorObj.SortColumn1.Name).Ascending(cursorObj.SortColumn2!.Value.Name),
                    Limit = limit,
                    Projection = projection
                };
                var items = await Collection.FindAsync(innerFilter, findOptions).ToListAsync();
                return new DataAndNext<TProjection>(items, items.Count != 0 ? cursorObj.Next(items.Last()).ToBase64Json() : null);
            }
        }

        private async Task<DataAndNext<TProjection>> FindAsync1<TCursor, TProjection>(ProjectionDefinition<TElement, TProjection> projection, Func<TCursor, Task<BsonDocument?>>? bsonFilter = null)
            where TProjection : class
            where TCursor : class, IPageCursor<TCursor, TProjection>
        {
            if (this.Cursor is not null)
            {
                var limit = this.Limit.Limit();
                var cursorObj = this.Cursor.FromBase64Json<TCursor>()!;
                var innerFilter = bsonFilter is not null ? await bsonFilter(cursorObj) : new BsonDocument();
                if (innerFilter is null)
                    return new DataAndNext<TProjection>(new List<TProjection>(), null);
                var filter = BSON.Create(b => b.And(
                    innerFilter,
                    new Dictionary<string, object> { [cursorObj.SortColumn1.Name] = b.Gt(cursorObj.SortColumn1.Value) }));

                var findOptions = new FindOptions<TElement, TProjection>()
                {
                    Sort = Builders<TElement>.Sort.Ascending(cursorObj.SortColumn1.Name),
                    Limit = limit,
                    Projection = projection
                };
                var items = await Collection.FindAsync(filter, findOptions).ToListAsync();
                return new DataAndNext<TProjection>(items, items.Count != 0 ? cursorObj.Next(items.Last()).ToBase64Json() : null);
            }
            else
            {
                var limit = this.Limit.Limit();
                var cursorObj = (TCursor)TCursor.FromFilter(this.Filter);
                var innerFilter = bsonFilter is not null ? await bsonFilter(cursorObj) : new BsonDocument();
                if (innerFilter is null)
                    return new DataAndNext<TProjection>(new List<TProjection>(), null);

                var findOptions = new FindOptions<TElement, TProjection>()
                {
                    Sort = Builders<TElement>.Sort.Ascending(cursorObj.SortColumn1.Name),
                    Limit = limit,
                    Projection = projection
                };
                var items = await Collection.FindAsync(innerFilter, findOptions).ToListAsync();
                return new DataAndNext<TProjection>(items, items.Count != 0 ? cursorObj.Next(items.Last()).ToBase64Json() : null);
            }
        }
    }

    public record DataAndNext<T>(List<T> Data, string? Next);
}
