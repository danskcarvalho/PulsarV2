using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulsar.BuildingBlocks.DDD.Mongo.Queries
{
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

        public IMongoCollection<TModel> GetCollection<TModel>(string collectionName)
        {
            var col = Database.GetCollection<TModel>(collectionName);
            col = col.WithWriteConcern(WriteConcern.WMajority).WithReadConcern(ReadConcern.Majority).WithReadPreference(ReadPreference.Secondary);
            return col;
        }

        public QueryHandler(MongoDbSessionFactory factory)
        {
            Factory = factory;
        }

        public bool IsCausalllyConsistent => CurrentHandle?.Options?.CausalConsistency == true;

        public string? ConsistencyToken => GetConsistencyToken(CurrentHandle.ClusterTime, CurrentHandle.OperationTime);

        private string? GetConsistencyToken(BsonDocument? clusterTime, BsonTimestamp? operationTime)
        {
            if (clusterTime is null || operationTime is null)
                return null;

            var bson = new BsonDocument
        {
            { "clusterTime", clusterTime },
            { "operationTime", operationTime  }
        };

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(bson.ToJson()));
        }

        private bool ParseConsistencyToken(string consistencyToken, out BsonDocument? clusterTime, out BsonTimestamp? operationTime)
        {
            clusterTime = null;
            operationTime = null;
            try
            {
                if (string.IsNullOrWhiteSpace(consistencyToken))
                    return false;
                var json = Encoding.UTF8.GetString(Convert.FromBase64String(consistencyToken));
                var doc = json.ToBsonDocument();
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
            if (IsCausalllyConsistent)
                throw new InvalidOperationException("already causally consistent");

            BsonDocument? clusterTime = null;
            BsonTimestamp? operationTime = null;
            if (consistencyToken != null && !ParseConsistencyToken(consistencyToken, out clusterTime, out operationTime))
                throw new InvalidOperationException("invalid consistency token");

            if (_baseSession == null)
            {
                _baseSession = await Client.StartSessionAsync(cancellationToken: ct);
            }

            _ccSession = await Client.StartSessionAsync(new ClientSessionOptions()
            {
                CausalConsistency = true
            }, ct);


            if (consistencyToken != null)
            {
                if (clusterTime != null)
                    _ccSession.AdvanceClusterTime(clusterTime);
                if (operationTime != null)
                    _ccSession.AdvanceOperationTime(operationTime);
            }
            else
            {
                if (_baseSession.ClusterTime != null)
                    _ccSession.AdvanceClusterTime(_baseSession.ClusterTime);
                if (_baseSession.OperationTime != null)
                    _ccSession.AdvanceOperationTime(_baseSession.OperationTime);
            }

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
}
