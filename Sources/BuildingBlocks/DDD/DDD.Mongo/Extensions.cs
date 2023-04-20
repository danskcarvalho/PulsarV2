using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulsar.BuildingBlocks.DDD.Mongo
{
    public static class Extensions
    {
        public async static Task<List<TDocument>> ToListAsync<TDocument>(this Task<IAsyncCursor<TDocument>> cursor)
        {
            return await (await cursor).ToListAsync();
        }

        public async static Task<TDocument?> FirstOrDefaultAsync<TDocument>(this Task<IAsyncCursor<TDocument>> cursor) where TDocument : class
        {
            return await (await cursor).FirstOrDefaultAsync();
        }

        public static FilterDefinition<T> ToTextSearch<T>(this string? term) where T : class
        {
            if (term == null || term.IsEmpty())
                return FilterDefinition<T>.Empty;

            var parts = term.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var searchTerms = new List<FilterDefinition<T>>();
            foreach (var p in parts)
            {
                if (p.Length < 3)
                    continue;

                searchTerms.Add(Builders<T>.Filter.Text(p.ToLowerInvariant()));
            }

            return Builders<T>.Filter.And(searchTerms.ToArray());
        }
    }
}
