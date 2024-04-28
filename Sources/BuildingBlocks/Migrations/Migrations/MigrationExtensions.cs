using Pulsar.BuildingBlocks.DDD;
using Pulsar.BuildingBlocks.DDD.Mongo;

namespace Pulsar.BuildingBlocks.Migrations
{
    public static class MigrationsExtensions
    {
        public static async Task UpIndexes(this Migration mig, params Assembly[] assembliesToScan)
        {
            if (mig.Database == null)
                return;

            foreach (var assembly in assembliesToScan)
            {
                var descriptors = IndexDescriptions.AllDescriptors(assembly).ToList();

                foreach (var desc in descriptors)
                {
                    await MigrateIndexesForCollection(desc.ModelType, mig, desc);
                }
            }
        }

        private static Task MigrateIndexesForCollection(Type type, Migration mig, IndexDescriptions descriptions)
        {
            var method = typeof(MigrationsExtensions).GetMethod("MigrateIndexesForCollectionForT", BindingFlags.Static | BindingFlags.NonPublic) ?? throw new InvalidOperationException();
            return (method.MakeGenericMethod(type).Invoke(null, new object[] { mig, descriptions }) as Task) ?? throw new InvalidOperationException();
        }

        // DONT REMOVE: This is called via reflection
        private static async Task MigrateIndexesForCollectionForT<T>(Migration mig, IndexDescriptions descriptions)
        {
            var collection = mig.Database.GetCollection<T>(descriptions.CollectionName);
            var allDbIndexes = await collection.Indexes.ListAsync().ToListAsync();
            var dbNames = allDbIndexes.Select(f => f["name"].AsString).Select(n => n.ToLowerInvariant());
            var allIndexes = descriptions.AllIndexes();

            var toBeDeleted = new HashSet<string>(dbNames);
            toBeDeleted.Remove("_id_");
            foreach (var idx in allIndexes)
            {
                toBeDeleted.Remove($"ix_{idx.Key}");
            }

            foreach (var d in toBeDeleted)
            {
                await collection.Indexes.DropOneAsync(d);
            }

            foreach (var idx in allIndexes)
            {
                var b = (MongoIndexBuilder<T>)idx.Value;
                await collection.Indexes.CreateOneAsync(new MongoDB.Driver.CreateIndexModel<T>(b.GetDefinition(), new CreateIndexOptions()
                {
                    Unique = b.IsUnique,
                    Name = $"ix_{idx.Key}",
                    Collation = b.IsText ? Collation.Simple : new Collation("pt", caseLevel: false)
                }));
            }
        }
    }
}
