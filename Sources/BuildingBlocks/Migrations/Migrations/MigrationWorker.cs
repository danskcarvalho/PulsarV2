namespace Pulsar.BuildingBlocks.Migrations;

class MigrationWorker
{
    public int MigrationNumber { get; }
    public int TotalMigrations { get; }
    public Type MigrationType { get; }
    public MongoDbSessionFactory Factory { get; }
    public bool IsTestingEnvironment { get; }

    public MigrationWorker(
        int migrationNumber,
        int totalMigrations,
        Type migrationType,
        MongoDbSessionFactory factory,
        bool isTestingEnvironment)
    {
        MigrationNumber = migrationNumber;
        TotalMigrations = totalMigrations;
        MigrationType = migrationType;
        Factory = factory;
        IsTestingEnvironment = isTestingEnvironment;
    }

    public async Task Migrate()
    {
        var isForTesting = GetIsForTesting(MigrationType);
        if (isForTesting && !IsTestingEnvironment)
        {
            Print($"[{MigrationNumber}/{TotalMigrations}] skipping {GetVersion(MigrationType)}:{MigrationType.Name}", ConsoleColor.Yellow);
            return;
        }
        var startTransaction = GetRequiresTransaction(MigrationType);
        var isPersistent = GetIsPersistent(MigrationType);
        using var session = (MongoDbSession)Factory.CreateSession();

        if (startTransaction)
        {
            await RunMigrationInTransaction(isPersistent, session);
        }
        else
        {
            await RunMigrationOutsideOfTransaction(isPersistent, session);
        }
    }

    private async Task RunMigrationOutsideOfTransaction(bool isPersistent, MongoDbSession session)
    {
        var migration = Activator.CreateInstance(MigrationType) as Migration;
        if (migration == null)
            throw new InvalidOperationException("invalid migration type");

        var model = new MigrationModel()
        {
            CreatedOn = DateTime.UtcNow,
            Name = MigrationType.Name,
            Version = GetVersion(MigrationType)
        };
        Print($"[{MigrationNumber}/{TotalMigrations}] executing {model.Version}:{MigrationType.Name}", ConsoleColor.Green);
        //insert into the collection _Migrations

        try
        {
            migration.Set(Factory.Client, Factory.Database, session.CurrentHandle);
            await migration.Up();
        }
        catch (Exception e)
        {
            Print($"migration {model.Version}:{MigrationType.Name} failed with exception {e.GetType().FullName}", ConsoleColor.Red);
            Print($"migration {model.Version}:{MigrationType.Name} did not run under a transaction", ConsoleColor.Red);
            Print(ToJson(e), ConsoleColor.Red);
            throw;
        }

        if (!isPersistent) // if it's not persistent, then store in _Migrations collection so we don't rerun the migration
        {
            try
            {
                var collection = session.Database.GetCollection<MigrationModel>(MigrationConstants.COLLECTION_NAME);
                await collection.InsertOneAsync(session.CurrentHandle, model);
            }
            catch (Exception e)
            {
                Print($"failed to register migration {model.Version}:{MigrationType.Name} due to exception {e.GetType().FullName}", ConsoleColor.Red);
                Print(ToJson(e), ConsoleColor.Red);
                throw;
            }
        }
    }

    private async Task RunMigrationInTransaction(bool isPersistent, MongoDbSession session)
    {
        await session.OpenTransactionAsync(async ct =>
        {
            var migration = Activator.CreateInstance(MigrationType) as Migration;
            if (migration == null)
                throw new InvalidOperationException("invalid migration type");

            var model = new MigrationModel()
            {
                CreatedOn = DateTime.UtcNow,
                Name = MigrationType.Name,
                Version = GetVersion(MigrationType)
            };
            Print($"[{MigrationNumber}/{TotalMigrations}] executing {model.Version}:{MigrationType.Name}", ConsoleColor.Green);
            //insert into the collection _Migrations
            if (!isPersistent) // if it's not persistent, then store in _Migrations collection so we don't rerun the migration
            {
                var collection = session.Database.GetCollection<MigrationModel>(MigrationConstants.COLLECTION_NAME);
                await collection.InsertOneAsync(session.CurrentHandle, model, cancellationToken: ct);
            }

            try
            {
                migration.Set(Factory.Client, Factory.Database, session.CurrentHandle);
                await migration.Up();
            }
            catch (Exception e)
            {
                Print($"migration {model.Version}:{MigrationType.Name} failed with exception {e.GetType().FullName}", ConsoleColor.Red);
                Print(ToJson(e), ConsoleColor.Red);
                throw;
            }

            return 0;
        }, IsolationLevel.Committed);
    }

    private static string ToJson(Exception e)
    {
        try
        {
            return JsonConvert.SerializeObject(e);
        }
        catch
        {
            return JsonConvert.SerializeObject(new
            {
                e.Message,
                e.StackTrace
            });
        }
    }
    private static void Print(string msg, ConsoleColor color)
    {
        var previousForegroundColor = Console.ForegroundColor;
        try
        {
            Console.ForegroundColor = color;
            Console.WriteLine(msg);
        }
        finally
        {
            Console.ForegroundColor = previousForegroundColor;
        }
    }
    private static long GetVersion(Type migrationType)
    {
        var attr = migrationType
            .GetCustomAttributes(false)
            .Where(attr => attr is MigrationAttribute).Select(attr => attr as MigrationAttribute).First();
        return attr!.Version;
    }
    private static bool GetRequiresTransaction(Type migrationType)
    {
        var attr = migrationType
            .GetCustomAttributes(false)
            .Where(attr => attr is MigrationAttribute).Select(attr => attr as MigrationAttribute).First();
        return attr!.RequiresTransaction;
    }

    private static bool GetIsPersistent(Type migrationType)
    {
        var attr = migrationType
            .GetCustomAttributes(false)
            .Where(attr => attr is MigrationAttribute).Select(attr => attr as MigrationAttribute).First();
        return attr!.IsPersistent;
    }
    private static bool GetIsForTesting(Type migrationType)
    {
        var attr = migrationType
            .GetCustomAttributes(false)
            .Where(attr => attr is MigrationAttribute).Select(attr => attr as MigrationAttribute).First();
        return attr!.IsForTesting;
    }
}
