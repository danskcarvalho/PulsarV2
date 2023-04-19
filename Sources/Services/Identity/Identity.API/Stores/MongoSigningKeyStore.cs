using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;
using MongoDB.Driver.Core.Configuration;
using Pulsar.BuildingBlocks.EventBus.Abstractions;
using StackExchange.Redis;

namespace Pulsar.Services.Identity.API.Stores;

public class MongoSigningKeyStore : ISigningKeyStore
{
    private readonly MongoSigningKeyStoreConnection _connection;

    public MongoSigningKeyStore(MongoSigningKeyStoreConnection connection)
    {
        _connection = connection;
    }

    public async Task DeleteKeyAsync(string id)
    {
        await _connection.Collection.DeleteOneAsync(s => s.Id == id);
    }

    public async Task<IEnumerable<SerializedKey>> LoadKeysAsync()
    {
        return await _connection.Collection.FindAsync(s => true).ToListAsync();
    }

    public async Task StoreKeyAsync(SerializedKey key)
    {
        await _connection.Collection.InsertOneAsync(key);
    }
}

public class MongoSigningKeyStoreConnection
{
    private readonly IMongoClient _Client;
    private readonly IMongoDatabase _Database;
    private readonly IMongoCollection<SerializedKey> _Collection;
    private const string COLLECTION_NAME = "_KeyStore";
    public MongoSigningKeyStoreConnection(IConfiguration configuration)
    {
        var connectionString = configuration.GetOrThrow("MongoDB:ConnectionString");
        var database = configuration.GetOrThrow("MongoDB:Database");
        var settings = MongoClientSettings.FromConnectionString(connectionString) ?? throw new InvalidOperationException("invalid connection string");

        _Client = new MongoClient(settings);
        _Database = _Client.GetDatabase(database);
        _Collection = _Database.GetCollection<SerializedKey>(COLLECTION_NAME)
            .WithWriteConcern(WriteConcern.WMajority)
            .WithReadConcern(ReadConcern.Majority)
            .WithReadPreference(ReadPreference.PrimaryPreferred);
    }

    public IMongoCollection<SerializedKey> Collection => _Collection;
}
