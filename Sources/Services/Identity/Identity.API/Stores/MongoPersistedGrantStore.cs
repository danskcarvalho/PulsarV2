using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;

namespace Pulsar.Services.Identity.API.Stores;

public class MongoPersistedGrantStore : IPersistedGrantStore
{
    private readonly MongoStoreConnection _connection;

    public MongoPersistedGrantStore(MongoStoreConnection connection)
    {
        _connection = connection;
    }

    public async Task<IEnumerable<PersistedGrant>> GetAllAsync(PersistedGrantFilter filter)
    {
        return (await _connection.PersistedGrantStoreCollection.FindAsync(GetFilterDefinition(filter))).ToEnumerable();
    }

    public async Task<PersistedGrant> GetAsync(string key)
    {
        var r = await _connection.PersistedGrantStoreCollection.FindAsync(g => g.Key == key, new FindOptions<MongoPersistedGrant, MongoPersistedGrant>() { Limit = 1 }).FirstOrDefaultAsync();
        return r!;
    }

    public async Task RemoveAllAsync(PersistedGrantFilter filter)
    {
        var filterDefinition = GetFilterDefinition(filter);
        await _connection.PersistedGrantStoreCollection.DeleteManyAsync(filterDefinition);
    }

    private FilterDefinition<MongoPersistedGrant> GetFilterDefinition(PersistedGrantFilter filter)
    {
        List<FilterDefinition<MongoPersistedGrant>> definitions = new List<FilterDefinition<MongoPersistedGrant>>();
        if (filter.SubjectId is not null)
        {
            definitions.Add(Builders<MongoPersistedGrant>.Filter.Eq(x => x.SubjectId, filter.SubjectId));
        }
        if (filter.ClientId is not null)
        {
            definitions.Add(Builders<MongoPersistedGrant>.Filter.Eq(x => x.ClientId, filter.ClientId));
        }
        if (filter.SessionId is not null)
        {
            definitions.Add(Builders<MongoPersistedGrant>.Filter.Eq(x => x.SessionId, filter.SessionId));
        }
        if (filter.Type is not null)
        {
            definitions.Add(Builders<MongoPersistedGrant>.Filter.Eq(x => x.Type, filter.Type));
        }
        if (filter.ClientIds is not null && filter.ClientIds.Count() != 0)
        {
            definitions.Add(Builders<MongoPersistedGrant>.Filter.In(x => x.ClientId, filter.ClientIds));
        }
        if (filter.Types is not null && filter.Types.Count() != 0)
        {
            definitions.Add(Builders<MongoPersistedGrant>.Filter.In(x => x.Type, filter.Types));
        }

        if (definitions.Count == 0)
            return definitions.First();
        else
            return Builders<MongoPersistedGrant>.Filter.And(definitions);
    }

    public async Task RemoveAsync(string key)
    {
        await _connection.PersistedGrantStoreCollection.DeleteOneAsync(g => g.Key == key);
    }

    public async Task StoreAsync(PersistedGrant grant)
    {
        await _connection.PersistedGrantStoreCollection.InsertOneAsync(new MongoPersistedGrant(grant));
    }
}
