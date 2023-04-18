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
    }
}
