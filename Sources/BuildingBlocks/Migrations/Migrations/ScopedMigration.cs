using Pulsar.BuildingBlocks.DDD.Mongo;

namespace Pulsar.BuildingBlocks.Migrations;

class ScopedMigration
{
    private readonly MongoDbSessionFactory _sessionFactory;
    private const string TEST_COLLECTION = "_TestsMarker";
    public ScopedMigration(MongoDbSessionFactory sessionFactory)
    {
        _sessionFactory = sessionFactory;
    }

    public async Task Run(Assembly assembly, bool isTestingEnvironment)
    {
        using var gatheringSession = (MongoDbSession)_sessionFactory.CreateSession();
        if (isTestingEnvironment)
        {
            await ResetDatabase(gatheringSession);
        }

        var migrations = await GatherMigrations(assembly, gatheringSession);

        //run migrations
        int number = 1;
        foreach (var mig in migrations)
        {
            var worker = new MigrationWorker(number, migrations.Count, mig, _sessionFactory, isTestingEnvironment);
            await worker.Migrate();
            number++;
        }
    }

    private async Task ResetDatabase(MongoDbSession session)
    {
        var db = session.Database;
        if (!await db.CollectionExists(TEST_COLLECTION))
            await db.CreateCollectionAsync(TEST_COLLECTION);

        var collectionNames = await db.ListCollectionNamesAsync().ToListAsync();
        foreach (var name in collectionNames)
        {
            if (name == TEST_COLLECTION)
                continue;

            await db.DropCollectionAsync(name);
        }
    }

    private async Task<List<Type>> GatherMigrations(Assembly assembly, MongoDbSession session)
    {
        var allMigrationTypes = assembly.GetTypes().ToList();
        allMigrationTypes = allMigrationTypes.Where(t => IsMigrationType(t)).ToList();
        var orderedMigrations = allMigrationTypes.Select(t => (Type: t, Attribute: GetAttribute(t)))
            .OrderBy(t => t.Attribute!.Version).ToList();

        await CreateMigrationsCollectionIfNotExists(session);
        var migrations = await GetCurrentMigrations(session);
        var allVersions = new HashSet<long>(migrations.Select(m => m.Version));

        List<Type> result = new List<Type>();
        foreach (var migrationTy in orderedMigrations)
        {
            if (!allVersions.Contains(migrationTy.Attribute!.Version))
                result.Add(migrationTy.Type);
        }

        return result;
    }

    private static MigrationAttribute? GetAttribute(Type t)
    {
        return t.GetCustomAttribute<MigrationAttribute>();
    }

    private static bool IsMigrationType(Type t)
    {
        return t.IsClass && !t.IsGenericTypeDefinition && t.GetCustomAttributes<MigrationAttribute>().Any() && typeof(Migration).IsAssignableFrom(t);
    }

    private async Task<List<MigrationModel>> GetCurrentMigrations(MongoDbSession session)
    {
        var collection = session.Database.GetCollection<MigrationModel>(MigrationConstants.COLLECTION_NAME);
        return await (await collection.FindAsync(m => true)).ToListAsync();
    }

    private async Task CreateMigrationsCollectionIfNotExists(MongoDbSession session)
    {
        //check for existence of collection _Migrations
        var filter = new BsonDocument("name", MigrationConstants.COLLECTION_NAME);
        var collections = await session.Database.ListCollectionsAsync(new ListCollectionsOptions { Filter = filter });
        var exists = await collections.AnyAsync();
        if (!exists)
            await session.Database.CreateCollectionAsync(MigrationConstants.COLLECTION_NAME);
        //creates unique index to ensure that version is always unique
        //indexes are indempotent
        var collection = session.Database.GetCollection<MigrationModel>(MigrationConstants.COLLECTION_NAME);
        var keys = Builders<MigrationModel>.IndexKeys.Ascending(x => x.Version);
        await collection.Indexes.CreateOneAsync(new CreateIndexModel<MigrationModel>(keys, new CreateIndexOptions()
        {
            Unique = true,
            Name = MigrationConstants.VERSION_IS_UNIQUE_INDEX_NAME
        }));
    }
}
