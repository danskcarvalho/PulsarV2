using Pulsar.BuildingBlocks.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulsar.BuildingBlocks.DDD.Mongo.Cursors
{
    public record MongoDbCollectionWithCursor<TElement, TFilter> 
        where TElement : class
    {
        public required IMongoCollection<TElement> Collection { get; init; }
        public required IPageCursor<TElement, TFilter> Cursor { get; init; }

        public Task<DataAndNext<TElement>> FindAsync(Func<TFilter?, FilterDefinition<TElement>?>? filterFunction = null)
        {
            if (Cursor.HasSortField2)
                return FindAsync2(filterFunction);
            else
                return FindAsync1(filterFunction);
        }

        public Task<DataAndNext<TProjection>> FindAsync<TProjection>(ProjectionDefinition<TElement, TProjection> projection, Func<TFilter?, FilterDefinition<TElement>?>? filterFunction = null)
            where TProjection : class
        {
            if (Cursor.HasSortField2)
                return FindAsync2(projection, filterFunction);
            else
                return FindAsync1(projection, filterFunction);
        }
        public Task<DataAndNext<TElement>> FindAsyncWithAsyncFilter(Func<TFilter?, Task<FilterDefinition<TElement>?>>? filterFunction = null)
        {
            if (Cursor.HasSortField2)
                return FindAsync2(filterFunction);
            else
                return FindAsync1(filterFunction);
        }

        public Task<DataAndNext<TProjection>> FindAsyncWithAsyncFilter<TProjection>(ProjectionDefinition<TElement, TProjection> projection, Func<TFilter?, Task<FilterDefinition<TElement>?>>? filterFunction = null)
            where TProjection : class
        {
            if (Cursor.HasSortField2)
                return FindAsync2(projection, filterFunction);
            else
                return FindAsync1(projection, filterFunction);
        }

        private async Task<DataAndNext<TElement>> FindAsync2(Func<TFilter?, FilterDefinition<TElement>?>? filterFunction = null)
        {
            if (this.Cursor.TokenIsNotNull)
            {
                var limit = this.Cursor.Limit;
                var cursorObj = this.Cursor;
                var innerFilter = filterFunction is not null ? filterFunction(this.Cursor.Filter) : FilterDefinition<TElement>.Empty;
                if (innerFilter is null)
                    return new DataAndNext<TElement>(new List<TElement>(), null);

                var filter = Builders<TElement>.Filter.And(
                    innerFilter,
                    Builders<TElement>.Filter.Or(Builders<TElement>.Filter.Gt(cursorObj.SortField1.Name, cursorObj.SortField1.Value),
                        Builders<TElement>.Filter.And(Builders<TElement>.Filter.Eq(cursorObj.SortField1.Name, cursorObj.SortField1.Value), Builders<TElement>.Filter.Gt(cursorObj.SortField2!.Value.Name, cursorObj.SortField2!.Value.Value))));

                var findOptions = new FindOptions<TElement, TElement>()
                {
                    Sort = Builders<TElement>.Sort.Ascending(cursorObj.SortField1.Name).Ascending(cursorObj.SortField2!.Value.Name),
                    Limit = limit
                };
                var items = await Collection.FindAsync(filter, findOptions).ToListAsync();
                return new DataAndNext<TElement>(items, items.Count != 0 ? cursorObj.Next(items.Last()) : null);
            }
            else
            {
                var limit = this.Cursor.Limit;
                var innerFilter = filterFunction is not null ? filterFunction(this.Cursor.Filter) : FilterDefinition<TElement>.Empty;
                if (innerFilter is null)
                    return new DataAndNext<TElement>(new List<TElement>(), null);

                var findOptions = new FindOptions<TElement, TElement>()
                {
                    Sort = Builders<TElement>.Sort.Ascending(this.Cursor.SortField1.Name).Ascending(this.Cursor.SortField2!.Value.Name),
                    Limit = limit
                };
                var items = await Collection.FindAsync(innerFilter, findOptions).ToListAsync();
                return new DataAndNext<TElement>(items, items.Count != 0 ? this.Cursor.Next(items.Last()) : null);
            }
        }

        private async Task<DataAndNext<TElement>> FindAsync1(Func<TFilter?, FilterDefinition<TElement>?>? filterFunction = null)
        {
            if (this.Cursor.TokenIsNotNull)
            {
                var limit = this.Cursor.Limit;
                var cursorObj = this.Cursor;
                var innerFilter = filterFunction is not null ? filterFunction(this.Cursor.Filter) : FilterDefinition<TElement>.Empty;
                if (innerFilter is null)
                    return new DataAndNext<TElement>(new List<TElement>(), null);
                var filter = Builders<TElement>.Filter.And(
                    innerFilter,
                    Builders<TElement>.Filter.Gt(cursorObj.SortField1.Name, cursorObj.SortField1.Value));

                var findOptions = new FindOptions<TElement, TElement>()
                {
                    Sort = Builders<TElement>.Sort.Ascending(cursorObj.SortField1.Name),
                    Limit = limit
                };
                var items = await Collection.FindAsync(filter, findOptions).ToListAsync();
                return new DataAndNext<TElement>(items, items.Count != 0 ? cursorObj.Next(items.Last()) : null);
            }
            else
            {
                var limit = this.Cursor.Limit;
                var cursorObj = this.Cursor;
                var innerFilter = filterFunction is not null ? filterFunction(cursorObj.Filter) : FilterDefinition<TElement>.Empty;
                if (innerFilter is null)
                    return new DataAndNext<TElement>(new List<TElement>(), null);

                var findOptions = new FindOptions<TElement, TElement>()
                {
                    Sort = Builders<TElement>.Sort.Ascending(cursorObj.SortField1.Name),
                    Limit = limit
                };
                var items = await Collection.FindAsync(innerFilter, findOptions).ToListAsync();
                return new DataAndNext<TElement>(items, items.Count != 0 ? cursorObj.Next(items.Last()) : null);
            }
        }

        private async Task<DataAndNext<TProjection>> FindAsync2<TProjection>(ProjectionDefinition<TElement, TProjection> projection, Func<TFilter?, FilterDefinition<TElement>?>? filterFunction = null)
            where TProjection : class
        {
            if (this.Cursor.TokenIsNotNull)
            {
                var limit = this.Cursor.Limit;
                var cursorObj = this.Cursor;
                var innerFilter = filterFunction is not null ? filterFunction(this.Cursor.Filter) : FilterDefinition<TElement>.Empty;
                if (innerFilter is null)
                    return new DataAndNext<TProjection>(new List<TProjection>(), null);
                var filter = Builders<TElement>.Filter.And(
                    innerFilter,
                    Builders<TElement>.Filter.Or(Builders<TElement>.Filter.Gt(cursorObj.SortField1.Name, cursorObj.SortField1.Value),
                        Builders<TElement>.Filter.And(Builders<TElement>.Filter.Eq(cursorObj.SortField1.Name, cursorObj.SortField1.Value), Builders<TElement>.Filter.Gt(cursorObj.SortField2!.Value.Name, cursorObj.SortField2!.Value.Value))));

                var findOptions = new FindOptions<TElement, TProjection>()
                {
                    Sort = Builders<TElement>.Sort.Ascending(cursorObj.SortField1.Name).Ascending(cursorObj.SortField2!.Value.Name),
                    Limit = limit,
                    Projection = projection
                };
                var items = await Collection.FindAsync(filter, findOptions).ToListAsync();
                return new DataAndNext<TProjection>(items, items.Count != 0 ? cursorObj.NextFromProjection(items.Last()) : null);
            }
            else
            {
                var limit = this.Cursor.Limit;
                var cursorObj = this.Cursor;
                var innerFilter = filterFunction is not null ? filterFunction(cursorObj.Filter) : FilterDefinition<TElement>.Empty;
                if (innerFilter is null)
                    return new DataAndNext<TProjection>(new List<TProjection>(), null);

                var findOptions = new FindOptions<TElement, TProjection>()
                {
                    Sort = Builders<TElement>.Sort.Ascending(cursorObj.SortField1.Name).Ascending(cursorObj.SortField2!.Value.Name),
                    Limit = limit,
                    Projection = projection
                };
                var items = await Collection.FindAsync(innerFilter, findOptions).ToListAsync();
                return new DataAndNext<TProjection>(items, items.Count != 0 ? cursorObj.NextFromProjection(items.Last()) : null);
            }
        }

        private async Task<DataAndNext<TProjection>> FindAsync1<TProjection>(ProjectionDefinition<TElement, TProjection> projection, Func<TFilter?, FilterDefinition<TElement>?>? filterFunction = null)
            where TProjection : class
        {
            if (this.Cursor.TokenIsNotNull)
            {
                var limit = this.Cursor.Limit;
                var cursorObj = this.Cursor;
                var innerFilter = filterFunction is not null ? filterFunction(this.Cursor.Filter) : FilterDefinition<TElement>.Empty;
                if (innerFilter is null)
                    return new DataAndNext<TProjection>(new List<TProjection>(), null);
                var filter = Builders<TElement>.Filter.And(
                    innerFilter,
                    Builders<TElement>.Filter.Gt(cursorObj.SortField1.Name, cursorObj.SortField1.Value));

                var findOptions = new FindOptions<TElement, TProjection>()
                {
                    Sort = Builders<TElement>.Sort.Ascending(cursorObj.SortField1.Name),
                    Limit = limit,
                    Projection = projection
                };
                var items = await Collection.FindAsync(filter, findOptions).ToListAsync();
                return new DataAndNext<TProjection>(items, items.Count != 0 ? cursorObj.NextFromProjection(items.Last()) : null);
            }
            else
            {
                var limit = this.Cursor.Limit;
                var cursorObj = this.Cursor;
                var innerFilter = filterFunction is not null ? filterFunction(cursorObj.Filter) : FilterDefinition<TElement>.Empty;
                if (innerFilter is null)
                    return new DataAndNext<TProjection>(new List<TProjection>(), null);

                var findOptions = new FindOptions<TElement, TProjection>()
                {
                    Sort = Builders<TElement>.Sort.Ascending(cursorObj.SortField1.Name),
                    Limit = limit,
                    Projection = projection
                };
                var items = await Collection.FindAsync(innerFilter, findOptions).ToListAsync();
                return new DataAndNext<TProjection>(items, items.Count != 0 ? cursorObj.NextFromProjection(items.Last()) : null);
            }
        }

        private async Task<DataAndNext<TElement>> FindAsync2(Func<TFilter?, Task<FilterDefinition<TElement>?>>? filterFunction = null)
        {
            if (this.Cursor.TokenIsNotNull)
            {
                var limit = this.Cursor.Limit;
                var cursorObj = this.Cursor;
                var innerFilter = filterFunction is not null ? await filterFunction(this.Cursor.Filter) : FilterDefinition<TElement>.Empty;
                if (innerFilter is null)
                    return new DataAndNext<TElement>(new List<TElement>(), null);

                var filter = Builders<TElement>.Filter.And(
                    innerFilter,
                    Builders<TElement>.Filter.Or(Builders<TElement>.Filter.Gt(cursorObj.SortField1.Name, cursorObj.SortField1.Value),
                        Builders<TElement>.Filter.And(Builders<TElement>.Filter.Eq(cursorObj.SortField1.Name, cursorObj.SortField1.Value), Builders<TElement>.Filter.Gt(cursorObj.SortField2!.Value.Name, cursorObj.SortField2!.Value.Value))));

                var findOptions = new FindOptions<TElement, TElement>()
                {
                    Sort = Builders<TElement>.Sort.Ascending(cursorObj.SortField1.Name).Ascending(cursorObj.SortField2!.Value.Name),
                    Limit = limit
                };
                var items = await Collection.FindAsync(filter, findOptions).ToListAsync();
                return new DataAndNext<TElement>(items, items.Count != 0 ? cursorObj.Next(items.Last()) : null);
            }
            else
            {
                var limit = this.Cursor.Limit;
                var cursorObj = this.Cursor;
                var innerFilter = filterFunction is not null ? await filterFunction(cursorObj.Filter) : FilterDefinition<TElement>.Empty;
                if (innerFilter is null)
                    return new DataAndNext<TElement>(new List<TElement>(), null);

                var findOptions = new FindOptions<TElement, TElement>()
                {
                    Sort = Builders<TElement>.Sort.Ascending(cursorObj.SortField1.Name).Ascending(cursorObj.SortField2!.Value.Name),
                    Limit = limit
                };
                var items = await Collection.FindAsync(innerFilter, findOptions).ToListAsync();
                return new DataAndNext<TElement>(items, items.Count != 0 ? cursorObj.Next(items.Last()) : null);
            }
        }

        private async Task<DataAndNext<TElement>> FindAsync1(Func<TFilter?, Task<FilterDefinition<TElement>?>>? filterFunction = null)
        {
            if (this.Cursor.TokenIsNotNull)
            {
                var limit = this.Cursor.Limit;
                var cursorObj = this.Cursor;
                var innerFilter = filterFunction is not null ? await filterFunction(this.Cursor.Filter) : FilterDefinition<TElement>.Empty;
                if (innerFilter is null)
                    return new DataAndNext<TElement>(new List<TElement>(), null);
                var filter = Builders<TElement>.Filter.And(
                    innerFilter,
                    Builders<TElement>.Filter.Gt(cursorObj.SortField1.Name, cursorObj.SortField1.Value));

                var findOptions = new FindOptions<TElement, TElement>()
                {
                    Sort = Builders<TElement>.Sort.Ascending(cursorObj.SortField1.Name),
                    Limit = limit
                };
                var items = await Collection.FindAsync(filter, findOptions).ToListAsync();
                return new DataAndNext<TElement>(items, items.Count != 0 ? cursorObj.Next(items.Last()) : null);
            }
            else
            {
                var limit = this.Cursor.Limit;
                var cursorObj = this.Cursor;
                var innerFilter = filterFunction is not null ? await filterFunction(cursorObj.Filter) : FilterDefinition<TElement>.Empty;
                if (innerFilter is null)
                    return new DataAndNext<TElement>(new List<TElement>(), null);

                var findOptions = new FindOptions<TElement, TElement>()
                {
                    Sort = Builders<TElement>.Sort.Ascending(cursorObj.SortField1.Name),
                    Limit = limit
                };
                var items = await Collection.FindAsync(innerFilter, findOptions).ToListAsync();
                return new DataAndNext<TElement>(items, items.Count != 0 ? cursorObj.Next(items.Last()) : null);
            }
        }

        private async Task<DataAndNext<TProjection>> FindAsync2<TProjection>(ProjectionDefinition<TElement, TProjection> projection, Func<TFilter?, Task<FilterDefinition<TElement>?>>? filterFunction = null)
            where TProjection : class
        {
            if (this.Cursor.TokenIsNotNull)
            {
                var limit = this.Cursor.Limit;
                var cursorObj = this.Cursor;
                var innerFilter = filterFunction is not null ? await filterFunction(this.Cursor.Filter) : FilterDefinition<TElement>.Empty;
                if (innerFilter is null)
                    return new DataAndNext<TProjection>(new List<TProjection>(), null);
                var filter = Builders<TElement>.Filter.And(
                    innerFilter,
                    Builders<TElement>.Filter.Or(Builders<TElement>.Filter.Gt(cursorObj.SortField1.Name, cursorObj.SortField1.Value),
                        Builders<TElement>.Filter.And(Builders<TElement>.Filter.Eq(cursorObj.SortField1.Name, cursorObj.SortField1.Value), Builders<TElement>.Filter.Gt(cursorObj.SortField2!.Value.Name, cursorObj.SortField2!.Value.Value))));

                var findOptions = new FindOptions<TElement, TProjection>()
                {
                    Sort = Builders<TElement>.Sort.Ascending(cursorObj.SortField1.Name).Ascending(cursorObj.SortField2!.Value.Name),
                    Limit = limit,
                    Projection = projection
                };
                var items = await Collection.FindAsync(filter, findOptions).ToListAsync();
                return new DataAndNext<TProjection>(items, items.Count != 0 ? cursorObj.NextFromProjection(items.Last()) : null);
            }
            else
            {
                var limit = this.Cursor.Limit;
                var cursorObj = this.Cursor;
                var innerFilter = filterFunction is not null ? await filterFunction(cursorObj.Filter) : FilterDefinition<TElement>.Empty;
                if (innerFilter is null)
                    return new DataAndNext<TProjection>(new List<TProjection>(), null);

                var findOptions = new FindOptions<TElement, TProjection>()
                {
                    Sort = Builders<TElement>.Sort.Ascending(cursorObj.SortField1.Name).Ascending(cursorObj.SortField2!.Value.Name),
                    Limit = limit,
                    Projection = projection
                };
                var items = await Collection.FindAsync(innerFilter, findOptions).ToListAsync();
                return new DataAndNext<TProjection>(items, items.Count != 0 ? cursorObj.NextFromProjection(items.Last()) : null);
            }
        }

        private async Task<DataAndNext<TProjection>> FindAsync1<TProjection>(ProjectionDefinition<TElement, TProjection> projection, Func<TFilter?, Task<FilterDefinition<TElement>?>>? filterFunction = null)
            where TProjection : class
        {
            if (this.Cursor.TokenIsNotNull)
            {
                var limit = this.Cursor.Limit;
                var cursorObj = this.Cursor;
                var innerFilter = filterFunction is not null ? await filterFunction(this.Cursor.Filter) : FilterDefinition<TElement>.Empty;
                if (innerFilter is null)
                    return new DataAndNext<TProjection>(new List<TProjection>(), null);
                var filter = Builders<TElement>.Filter.And(
                    innerFilter,
                    Builders<TElement>.Filter.Gt(cursorObj.SortField1.Name, cursorObj.SortField1.Value));

                var findOptions = new FindOptions<TElement, TProjection>()
                {
                    Sort = Builders<TElement>.Sort.Ascending(cursorObj.SortField1.Name),
                    Limit = limit,
                    Projection = projection
                };
                var items = await Collection.FindAsync(filter, findOptions).ToListAsync();
                return new DataAndNext<TProjection>(items, items.Count != 0 ? cursorObj.NextFromProjection(items.Last()) : null);
            }
            else
            {
                var limit = this.Cursor.Limit;
                var cursorObj = this.Cursor;
                var innerFilter = filterFunction is not null ? await filterFunction(cursorObj.Filter) : FilterDefinition<TElement>.Empty;
                if (innerFilter is null)
                    return new DataAndNext<TProjection>(new List<TProjection>(), null);

                var findOptions = new FindOptions<TElement, TProjection>()
                {
                    Sort = Builders<TElement>.Sort.Ascending(cursorObj.SortField1.Name),
                    Limit = limit,
                    Projection = projection
                };
                var items = await Collection.FindAsync(innerFilter, findOptions).ToListAsync();
                return new DataAndNext<TProjection>(items, items.Count != 0 ? cursorObj.NextFromProjection(items.Last()) : null);
            }
        }
    }

    public record DataAndNext<T>(List<T> Data, string? Next);
}
