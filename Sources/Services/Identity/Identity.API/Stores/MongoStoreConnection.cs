using Duende.IdentityServer.Models;

namespace Pulsar.Services.Identity.API.Stores;

public class MongoStoreConnection
{
    private readonly IMongoClient _Client;
    private readonly IMongoDatabase _Database;
    private readonly IMongoCollection<SerializedKey> _KeyStoreCollection;
    private readonly IMongoCollection<MongoPersistedGrant> _GrantStoreCollection;
    private const string KEY_STORE_COLLECTION_NAME = "_KeyStore";
    private const string GRANT_STORE_COLLECTION_NAME = "_GrantStore";
    public MongoStoreConnection(IConfiguration configuration)
    {
        var connectionString = configuration.GetOrThrow("MongoDB:ConnectionString");
        var database = configuration.GetOrThrow("MongoDB:Database");
        var settings = MongoClientSettings.FromConnectionString(connectionString) ?? throw new InvalidOperationException("invalid connection string");

        _Client = new MongoClient(settings);
        _Database = _Client.GetDatabase(database);
        _KeyStoreCollection = _Database.GetCollection<SerializedKey>(KEY_STORE_COLLECTION_NAME)
            .WithWriteConcern(WriteConcern.WMajority)
            .WithReadConcern(ReadConcern.Majority)
            .WithReadPreference(ReadPreference.PrimaryPreferred);
        _GrantStoreCollection = _Database.GetCollection<MongoPersistedGrant>(GRANT_STORE_COLLECTION_NAME)
            .WithWriteConcern(WriteConcern.WMajority)
            .WithReadConcern(ReadConcern.Majority)
            .WithReadPreference(ReadPreference.PrimaryPreferred);
    }

    public IMongoCollection<SerializedKey> KeyStoreCollection => _KeyStoreCollection;
    public IMongoCollection<MongoPersistedGrant> GrantStoreCollection => _GrantStoreCollection;
}
