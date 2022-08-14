namespace Pulsar.BuildingBlocks.DDD.Mongo.Implementations;

public class MongoDbSessionFactory : IDbSessionFactory
{
    public IMongoDatabase Database { get; }
    public IMongoClient Client { get; }
    public Func<IMediator> Mediator { get; } 

    public MongoDbSessionFactory(string connectionString, string database, Func<IMediator> mediator)
    {
        var settings = MongoClientSettings.FromConnectionString(connectionString) ?? throw new InvalidOperationException("invalid connection string");

        settings.RetryReads = true;
        settings.RetryWrites = true;
        settings.ReadConcern = ReadConcern.Majority;
        settings.WriteConcern = WriteConcern.WMajority;
        settings.ReadPreference = ReadPreference.Primary;
        settings.LinqProvider = MongoDB.Driver.Linq.LinqProvider.V3;

        Client = new MongoClient(settings);
        Database = Client.GetDatabase(database, new MongoDatabaseSettings()
        {
            ReadConcern = ReadConcern.Majority,
            WriteConcern = WriteConcern.WMajority,
            ReadPreference = ReadPreference.Primary
        });
        Mediator = mediator;
    }

    public IDbSession CreateSession()
    {
        return new MongoDbSession(this, Mediator());
    }
}
