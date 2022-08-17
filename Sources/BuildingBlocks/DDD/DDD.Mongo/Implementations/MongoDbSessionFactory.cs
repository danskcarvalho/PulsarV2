using MongoDB.Driver;
using MongoDB.Driver.Core.Events;
using System.Diagnostics;

namespace Pulsar.BuildingBlocks.DDD.Mongo.Implementations;

public class MongoDbSessionFactory : IDbSessionFactory
{
    private string? _clusterName;
    public IMongoDatabase Database { get; }
    public IMongoClient Client { get; }
    public Func<IMediator> Mediator { get; }

    public MongoDbSessionFactory(string connectionString, string database, string? clusterName, Func<IMediator> mediator)
    {
        var settings = MongoClientSettings.FromConnectionString(connectionString) ?? throw new InvalidOperationException("invalid connection string");

        settings.RetryReads = true;
        settings.RetryWrites = true;
        settings.ReadConcern = ReadConcern.Majority;
        settings.WriteConcern = WriteConcern.WMajority;
        settings.ReadPreference = ReadPreference.Primary;
        settings.LinqProvider = MongoDB.Driver.Linq.LinqProvider.V3;
#if DEBUG
        settings.ClusterConfigurator = cb => {
            cb.Subscribe<CommandStartedEvent>(e => {
                Debug.WriteLine($"{e.CommandName} - {e.Command.ToJson()}");
            });
        };
#endif

        Client = new MongoClient(settings);
        Database = Client.GetDatabase(database, new MongoDatabaseSettings()
        {
            ReadConcern = ReadConcern.Majority,
            WriteConcern = WriteConcern.WMajority,
            ReadPreference = ReadPreference.Primary
        });
        Mediator = mediator;
        _clusterName = clusterName;
    }

    public IDbSession CreateSession(IMediator? mediator = null)
    {
        return new MongoDbSession(this, mediator ?? Mediator(), _clusterName);
    }
}
