using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;
using MongoDB.Driver.Core.Configuration;
using Pulsar.BuildingBlocks.EventBus.Abstractions;
using StackExchange.Redis;

namespace Pulsar.Services.Identity.API.Stores;

public class MongoSigningKeyStore : ISigningKeyStore
{
    private readonly MongoStoreConnection _connection;

    public MongoSigningKeyStore(MongoStoreConnection connection)
    {
        _connection = connection;
    }

    public async Task DeleteKeyAsync(string id)
    {
        await _connection.KeyStoreCollection.DeleteOneAsync(s => s.Id == id);
    }

    public async Task<IEnumerable<SerializedKey>> LoadKeysAsync()
    {
        return await _connection.KeyStoreCollection.FindAsync(s => true).ToListAsync();
    }

    public async Task StoreKeyAsync(SerializedKey key)
    {
        await _connection.KeyStoreCollection.ReplaceOneAsync(f => f.Id == key.Id, key, new ReplaceOptions
        {
            IsUpsert = true
        });
    }
}
