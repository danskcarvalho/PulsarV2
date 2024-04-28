using Pulsar.BuildingBlocks.Sync.Contracts;

namespace Pulsar.BuildingBlocks.Migrations;

public abstract class Migration
{
    private IMongoClient? client;
    private IMongoDatabase? database;
    private IClientSessionHandle? session;

    public IMongoClient Client { get => client!; private set => client = value; }
    public IMongoDatabase Database { get => database!; private set => database = value; }
    public IClientSessionHandle Session { get => session!; private set => session = value; }

    internal void Set(IMongoClient client, IMongoDatabase db, IClientSessionHandle session)
    {
        Client = client;
        Database = db;
        Session = session;
    }

    public abstract Task Up();
    protected IMongoCollection<T> GetCollection<T>(string name) => Database!.GetCollection<T>(name);
    
    protected async Task CreateCollection(string collectionName, Collation collation)
    {
        await this.Database.CreateCollectionAsync(collectionName, new CreateCollectionOptions()
        {
            Collation = collation
        });
    }
    
    protected async Task CreateShadowCollection<TShadow>(Collation collation) where TShadow : class, IShadow
    {
        var collectionName = GetCollectionName<TShadow>();
        await this.Database.CreateCollectionAsync(collectionName, new CreateCollectionOptions()
        {
            Collation = collation
        });
    }
    
    private static string GetCollectionName<T>()
    {
        var attr = typeof(T).GetCustomAttribute<ShadowAttribute>();
        if (attr == null)
        {
            throw new InvalidOperationException($"no ShadowAttribute on type {typeof(T).FullName}");
        }

        return $"_{ValidId(attr.Name)}_Shadow";
    }

    private static string ValidId(string name)
    {
        return new string(name.Where(c => char.IsLetterOrDigit(c)).ToArray());
    }

}
