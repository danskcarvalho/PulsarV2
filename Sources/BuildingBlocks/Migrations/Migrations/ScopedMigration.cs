namespace Pulsar.BuildingBlocks.Migrations;

class ScopedMigration
{
    private readonly MongoDbSessionFactory _sessionFactory;

    public ScopedMigration(MongoDbSessionFactory sessionFactory)
    {
        _sessionFactory = sessionFactory;
    }

    public async Task Run(Assembly assembly)
    {
        using var gatheringSession = (MongoDbSession)_sessionFactory.CreateSession();
        var migrations = await GatherMigrations(assembly, gatheringSession);

        //run migrations
        int number = 1;
        foreach (var mig in migrations)
        {
            var worker = new MigrationWorker(number, migrations.Count, mig, _sessionFactory);
            await worker.Migrate();
            number++;
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
        var collection = session.Database.GetCollection<MigrationModel>(MigrationConstants.CollectionName);
        return await (await collection.FindAsync(m => true)).ToListAsync();
    }

    private async Task CreateMigrationsCollectionIfNotExists(MongoDbSession session)
    {
        //check for existence of collection _Migrations
        var filter = new BsonDocument("name", MigrationConstants.CollectionName);
        var collections = await session.Database.ListCollectionsAsync(new ListCollectionsOptions { Filter = filter });
        var exists = await collections.AnyAsync();
        if (!exists)
            await session.Database.CreateCollectionAsync(MigrationConstants.CollectionName);
        //creates unique index to ensure that version is always unique
        //indexes are indempotent
        var collection = session.Database.GetCollection<MigrationModel>(MigrationConstants.CollectionName);
        var keys = Builders<MigrationModel>.IndexKeys.Ascending(x => x.Version);
        await collection.Indexes.CreateOneAsync(new CreateIndexModel<MigrationModel>(keys, new CreateIndexOptions()
        {
            Unique = true,
            Name = MigrationConstants.VersionIsUniqueIndexName
        }));
    }
}
