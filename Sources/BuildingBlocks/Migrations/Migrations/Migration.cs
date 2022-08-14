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

}
