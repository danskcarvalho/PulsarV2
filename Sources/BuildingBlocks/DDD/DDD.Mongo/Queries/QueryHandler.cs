using Pulsar.BuildingBlocks.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulsar.BuildingBlocks.DDD.Mongo.Queries;

public class QueryHandler : IDisposable
{
    public MongoDbSessionFactory Factory { get; }
    private IMongoDatabase Database => Factory.Database;
    public IMongoClient Client => Factory.Client;
    public IClientSessionHandle CurrentHandle
    {
        get
        {
            if (_ccSession != null)
                return _ccSession;

            if (_baseSession != null)
                return _baseSession;

            _baseSession = Client.StartSession();
            return _baseSession;
        }
    }

    private IClientSessionHandle? _baseSession;
    private IClientSessionHandle? _ccSession = null;
    private string? _clusterName;

    public IMongoCollection<TModel> GetCollection<TModel>(string collectionName, ReadPref pref = ReadPref.Secondary)
    {
        var col = Database.GetCollection<TModel>(collectionName);
        col = col.WithWriteConcern(WriteConcern.WMajority).WithReadConcern(ReadConcern.Majority).WithReadPreference(pref switch
        {
            ReadPref.Nearest => ReadPreference.Nearest,
            ReadPref.Secondary => ReadPreference.SecondaryPreferred,
            ReadPref.Primary => ReadPreference.Primary,
            ReadPref.PrimaryPreferred => ReadPreference.PrimaryPreferred,
            _ => throw new InvalidOperationException()
        });
        return col;
    }

    public QueryHandler(MongoDbSessionFactory factory, string clusterName)
    {
        Factory = factory;
        _clusterName = clusterName;
    }

    public bool IsCausalllyConsistent => CurrentHandle?.Options?.CausalConsistency == true;

    private bool ParseConsistencyToken(string consistencyToken, out BsonDocument? clusterTime, out BsonTimestamp? operationTime)
    {
        clusterTime = null;
        operationTime = null;
        try
        {
            if (string.IsNullOrWhiteSpace(consistencyToken))
                return false;
            var json = Encoding.UTF8.GetString(consistencyToken.FromSafeBase64());
            var doc = BsonSerializer.Deserialize<BsonDocument>(json);
            var clusterName = doc["clusterName"].AsString;
            if (_clusterName != null && clusterName != _clusterName)
                throw new InvalidOperationException($"expected consistency token from cluster '{_clusterName}' but got '{clusterName}'");
            clusterTime = doc["clusterTime"].AsBsonDocument;
            operationTime = doc["operationTime"].AsBsonTimestamp;
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<TResult> StartCausallyConsistentSectionAsync<TResult>(Func<CancellationToken, Task<TResult>> action, string? consistencyToken = null, CancellationToken ct = default)
    {
        if (consistencyToken == null)
        {
            return await action(ct);
        }
        if (IsCausalllyConsistent)
            throw new InvalidOperationException("already causally consistent");

        BsonDocument? clusterTime = null;
        BsonTimestamp? operationTime = null;
        if (consistencyToken != null && !ParseConsistencyToken(consistencyToken, out clusterTime, out operationTime))
            throw new InvalidOperationException("invalid consistency token");

        if (_baseSession == null)
            _baseSession = await Client.StartSessionAsync(cancellationToken: ct);

        _ccSession = await Client.StartSessionAsync(new ClientSessionOptions()
        {
            CausalConsistency = true
        }, ct);


        if (clusterTime != null)
            _ccSession.AdvanceClusterTime(clusterTime);
        if (operationTime != null)
            _ccSession.AdvanceOperationTime(operationTime);

        try
        {
            return await action(ct);
        }
        finally
        {
            if (_ccSession.ClusterTime != null)
                _baseSession.AdvanceClusterTime(_ccSession.ClusterTime);
            if (_ccSession.OperationTime != null)
                _baseSession.AdvanceOperationTime(_ccSession.OperationTime);

            _ccSession.Dispose();
            _ccSession = null;
        }
    }

    public void Dispose()
    {
        if (_ccSession != null)
        {
            _ccSession.Dispose();
            _ccSession = null;
        }

        if (_baseSession != null)
        {
            _baseSession.Dispose();
            _baseSession = null;
        }
    }
}

public enum ReadPref
{
    Nearest = 1,
    Primary = 2,
    Secondary = 3,
    PrimaryPreferred = 4
}
