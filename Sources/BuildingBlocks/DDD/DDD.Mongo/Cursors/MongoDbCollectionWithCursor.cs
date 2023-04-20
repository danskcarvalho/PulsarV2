using Pulsar.BuildingBlocks.Utils;
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

        public Task<DataAndNext<TElement>> FindAsync<TCursor>(Func<TCursor, FilterDefinition<TElement>?>? filterFunction = null)
            where TCursor : class, IPageCursor<TCursor, TElement>
        {
            if (TCursor.HasSortColumn2)
                return FindAsync2(filterFunction);
            else
                return FindAsync1(filterFunction);
        }

        public Task<DataAndNext<TProjection>> FindAsync<TCursor, TProjection>(ProjectionDefinition<TElement, TProjection> projection, Func<TCursor, FilterDefinition<TElement>?>? filterFunction = null)
            where TProjection : class
            where TCursor : class, IPageCursor<TCursor, TProjection>
        {
            if (TCursor.HasSortColumn2)
                return FindAsync2(projection, filterFunction);
            else
                return FindAsync1(projection, filterFunction);
        }
        public Task<DataAndNext<TElement>> FindAsyncWithAsyncFilter<TCursor>(Func<TCursor, Task<FilterDefinition<TElement>?>>? filterFunction = null)
            where TCursor : class, IPageCursor<TCursor, TElement>
        {
            if (TCursor.HasSortColumn2)
                return FindAsync2(filterFunction);
            else
                return FindAsync1(filterFunction);
        }

        public Task<DataAndNext<TProjection>> FindAsyncWithAsyncFilter<TCursor, TProjection>(ProjectionDefinition<TElement, TProjection> projection, Func<TCursor, Task<FilterDefinition<TElement>?>>? filterFunction = null)
            where TProjection : class
            where TCursor : class, IPageCursor<TCursor, TProjection>
        {
            if (TCursor.HasSortColumn2)
                return FindAsync2(projection, filterFunction);
            else
                return FindAsync1(projection, filterFunction);
        }

        private async Task<DataAndNext<TElement>> FindAsync2<TCursor>(Func<TCursor, FilterDefinition<TElement>?>? filterFunction = null)
            where TCursor : class, IPageCursor<TCursor, TElement>
        {
            if (this.Cursor is not null)
            {
                var limit = this.Limit.Limit();
                var cursorObj = this.Cursor.FromBase64Json<TCursor>()!;
                var innerFilter = filterFunction is not null ? filterFunction(cursorObj) : FilterDefinition<TElement>.Empty;
                if (innerFilter is null)
                    return new DataAndNext<TElement>(new List<TElement>(), null);

                var filter = Builders<TElement>.Filter.And(
                    innerFilter,
                    Builders<TElement>.Filter.Or(Builders<TElement>.Filter.Gt(cursorObj.SortColumn1.Name, cursorObj.SortColumn1.Value),
                        Builders<TElement>.Filter.And(Builders<TElement>.Filter.Eq(cursorObj.SortColumn1.Name, cursorObj.SortColumn1.Value), Builders<TElement>.Filter.Gt(cursorObj.SortColumn2!.Value.Name, cursorObj.SortColumn2!.Value.Value))));

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
                var innerFilter = filterFunction is not null ? filterFunction(cursorObj) : FilterDefinition<TElement>.Empty;
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

        private async Task<DataAndNext<TElement>> FindAsync1<TCursor>(Func<TCursor, FilterDefinition<TElement>?>? filterFunction = null)
            where TCursor : class, IPageCursor<TCursor, TElement>
        {
            if (this.Cursor is not null)
            {
                var limit = this.Limit.Limit();
                var cursorObj = this.Cursor.FromBase64Json<TCursor>()!;
                var innerFilter = filterFunction is not null ? filterFunction(cursorObj) : FilterDefinition<TElement>.Empty;
                if (innerFilter is null)
                    return new DataAndNext<TElement>(new List<TElement>(), null);
                var filter = Builders<TElement>.Filter.And(
                    innerFilter,
                    Builders<TElement>.Filter.Gt(cursorObj.SortColumn1.Name, cursorObj.SortColumn1.Value));

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
                var innerFilter = filterFunction is not null ? filterFunction(cursorObj) : FilterDefinition<TElement>.Empty;
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

        private async Task<DataAndNext<TProjection>> FindAsync2<TCursor, TProjection>(ProjectionDefinition<TElement, TProjection> projection, Func<TCursor, FilterDefinition<TElement>?>? filterFunction = null)
            where TProjection : class
            where TCursor : class, IPageCursor<TCursor, TProjection>
        {
            if (this.Cursor is not null)
            {
                var limit = this.Limit.Limit();
                var cursorObj = this.Cursor.FromBase64Json<TCursor>()!;
                var innerFilter = filterFunction is not null ? filterFunction(cursorObj) : FilterDefinition<TElement>.Empty;
                if (innerFilter is null)
                    return new DataAndNext<TProjection>(new List<TProjection>(), null);
                var filter = Builders<TElement>.Filter.And(
                    innerFilter,
                    Builders<TElement>.Filter.Or(Builders<TElement>.Filter.Gt(cursorObj.SortColumn1.Name, cursorObj.SortColumn1.Value),
                        Builders<TElement>.Filter.And(Builders<TElement>.Filter.Eq(cursorObj.SortColumn1.Name, cursorObj.SortColumn1.Value), Builders<TElement>.Filter.Gt(cursorObj.SortColumn2!.Value.Name, cursorObj.SortColumn2!.Value.Value))));

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
                var innerFilter = filterFunction is not null ? filterFunction(cursorObj) : FilterDefinition<TElement>.Empty;
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

        private async Task<DataAndNext<TProjection>> FindAsync1<TCursor, TProjection>(ProjectionDefinition<TElement, TProjection> projection, Func<TCursor, FilterDefinition<TElement>?>? filterFunction = null)
            where TProjection : class
            where TCursor : class, IPageCursor<TCursor, TProjection>
        {
            if (this.Cursor is not null)
            {
                var limit = this.Limit.Limit();
                var cursorObj = this.Cursor.FromBase64Json<TCursor>()!;
                var innerFilter = filterFunction is not null ? filterFunction(cursorObj) : FilterDefinition<TElement>.Empty;
                if (innerFilter is null)
                    return new DataAndNext<TProjection>(new List<TProjection>(), null);
                var filter = Builders<TElement>.Filter.And(
                    innerFilter,
                    Builders<TElement>.Filter.Gt(cursorObj.SortColumn1.Name, cursorObj.SortColumn1.Value));

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
                var innerFilter = filterFunction is not null ? filterFunction(cursorObj) : FilterDefinition<TElement>.Empty;
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

        private async Task<DataAndNext<TElement>> FindAsync2<TCursor>(Func<TCursor, Task<FilterDefinition<TElement>?>>? filterFunction = null)
            where TCursor : class, IPageCursor<TCursor, TElement>
        {
            if (this.Cursor is not null)
            {
                var limit = this.Limit.Limit();
                var cursorObj = this.Cursor.FromBase64Json<TCursor>()!;
                var innerFilter = filterFunction is not null ? await filterFunction(cursorObj) : FilterDefinition<TElement>.Empty;
                if (innerFilter is null)
                    return new DataAndNext<TElement>(new List<TElement>(), null);

                var filter = Builders<TElement>.Filter.And(
                    innerFilter,
                    Builders<TElement>.Filter.Or(Builders<TElement>.Filter.Gt(cursorObj.SortColumn1.Name, cursorObj.SortColumn1.Value),
                        Builders<TElement>.Filter.And(Builders<TElement>.Filter.Eq(cursorObj.SortColumn1.Name, cursorObj.SortColumn1.Value), Builders<TElement>.Filter.Gt(cursorObj.SortColumn2!.Value.Name, cursorObj.SortColumn2!.Value.Value))));

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
                var innerFilter = filterFunction is not null ? await filterFunction(cursorObj) : FilterDefinition<TElement>.Empty;
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

        private async Task<DataAndNext<TElement>> FindAsync1<TCursor>(Func<TCursor, Task<FilterDefinition<TElement>?>>? filterFunction = null)
            where TCursor : class, IPageCursor<TCursor, TElement>
        {
            if (this.Cursor is not null)
            {
                var limit = this.Limit.Limit();
                var cursorObj = this.Cursor.FromBase64Json<TCursor>()!;
                var innerFilter = filterFunction is not null ? await filterFunction(cursorObj) : FilterDefinition<TElement>.Empty;
                if (innerFilter is null)
                    return new DataAndNext<TElement>(new List<TElement>(), null);
                var filter = Builders<TElement>.Filter.And(
                    innerFilter,
                    Builders<TElement>.Filter.Gt(cursorObj.SortColumn1.Name, cursorObj.SortColumn1.Value));

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
                var innerFilter = filterFunction is not null ? await filterFunction(cursorObj) : FilterDefinition<TElement>.Empty;
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

        private async Task<DataAndNext<TProjection>> FindAsync2<TCursor, TProjection>(ProjectionDefinition<TElement, TProjection> projection, Func<TCursor, Task<FilterDefinition<TElement>?>>? filterFunction = null)
            where TProjection : class
            where TCursor : class, IPageCursor<TCursor, TProjection>
        {
            if (this.Cursor is not null)
            {
                var limit = this.Limit.Limit();
                var cursorObj = this.Cursor.FromBase64Json<TCursor>()!;
                var innerFilter = filterFunction is not null ? await filterFunction(cursorObj) : FilterDefinition<TElement>.Empty;
                if (innerFilter is null)
                    return new DataAndNext<TProjection>(new List<TProjection>(), null);
                var filter = Builders<TElement>.Filter.And(
                    innerFilter,
                    Builders<TElement>.Filter.Or(Builders<TElement>.Filter.Gt(cursorObj.SortColumn1.Name, cursorObj.SortColumn1.Value),
                        Builders<TElement>.Filter.And(Builders<TElement>.Filter.Eq(cursorObj.SortColumn1.Name, cursorObj.SortColumn1.Value), Builders<TElement>.Filter.Gt(cursorObj.SortColumn2!.Value.Name, cursorObj.SortColumn2!.Value.Value))));

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
                var innerFilter = filterFunction is not null ? await filterFunction(cursorObj) : FilterDefinition<TElement>.Empty;
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

        private async Task<DataAndNext<TProjection>> FindAsync1<TCursor, TProjection>(ProjectionDefinition<TElement, TProjection> projection, Func<TCursor, Task<FilterDefinition<TElement>?>>? filterFunction = null)
            where TProjection : class
            where TCursor : class, IPageCursor<TCursor, TProjection>
        {
            if (this.Cursor is not null)
            {
                var limit = this.Limit.Limit();
                var cursorObj = this.Cursor.FromBase64Json<TCursor>()!;
                var innerFilter = filterFunction is not null ? await filterFunction(cursorObj) : FilterDefinition<TElement>.Empty;
                if (innerFilter is null)
                    return new DataAndNext<TProjection>(new List<TProjection>(), null);
                var filter = Builders<TElement>.Filter.And(
                    innerFilter,
                    Builders<TElement>.Filter.Gt(cursorObj.SortColumn1.Name, cursorObj.SortColumn1.Value));

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
                var innerFilter = filterFunction is not null ? await filterFunction(cursorObj) : FilterDefinition<TElement>.Empty;
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
