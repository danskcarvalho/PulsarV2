using Polly.Retry;
using System.Diagnostics;

namespace Pulsar.BuildingBlocks.DDD.Mongo.Implementations;

public class MongoDbSession : IDbSession
{
    public MongoDbSessionFactory Factory { get; }
    public IMongoDatabase Database => Factory.Database;
    public IMongoClient Client => Factory.Client;
    private Stack<IsolationLevel> _isolationLevelStack = new Stack<IsolationLevel>();
    public IsolationLevel? DefaultIsolationLevel => _isolationLevelStack.Count != 0 ? _isolationLevelStack.Peek() : null;
    public IClientSessionHandle CurrentHandle
    {
        get
        {
            if (_tranSession != null)
                return _tranSession;

            if (_ccSession != null)
                return _ccSession;

            if (_baseSession != null)
                return _baseSession;

            _baseSession = Client.StartSession();
            _trackedRoots.Push(new List<IAggregateRoot>());
            return _baseSession;
        }
    }

    private IClientSessionHandle? _baseSession;
    private IClientSessionHandle? _ccSession = null;
    private IClientSessionHandle? _tranSession = null;
    private IMediator _mediator;
    private Stack<List<IAggregateRoot>> _trackedRoots = new Stack<List<IAggregateRoot>>();
    private string? _clusterName;

    public MongoDbSession(MongoDbSessionFactory factory, IMediator mediator, string? clusterName)
    {
        Factory = factory;
        _mediator = mediator;
        _clusterName = clusterName;
    }

    public bool IsCausalllyConsistent => CurrentHandle?.Options?.CausalConsistency == true;

    public bool IsInTransaction => CurrentHandle?.IsInTransaction == true;

    public string? ConsistencyToken => GetConsistencyToken(CurrentHandle.ClusterTime, CurrentHandle.OperationTime);

    private string? GetConsistencyToken(BsonDocument? clusterTime, BsonTimestamp? operationTime)
    {
        if (clusterTime is null || operationTime is null)
            return null;

        var bson = new BsonDocument
        {
            { "clusterName", _clusterName },
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
        if (IsCausalllyConsistent)
            throw new InvalidOperationException("already causally consistent");
        if (IsInTransaction)
            throw new InvalidOperationException("in transaction");

        BsonDocument? clusterTime = null;
        BsonTimestamp? operationTime = null;
        if (consistencyToken != null && !ParseConsistencyToken(consistencyToken, out clusterTime, out operationTime))
            throw new InvalidOperationException("invalid consistency token");

        if (_baseSession == null)
        {
            _baseSession = await Client.StartSessionAsync(cancellationToken: ct);
            _trackedRoots.Push(new List<IAggregateRoot>());
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

        _trackedRoots.Push(new List<IAggregateRoot>());
        bool popped = false;
        try
        {
            var r = await action(ct);
            popped = true;
            await DispatchDomainEventsAndPopLastFrame();
            return r;
        }
        finally
        {
            if (!popped)
                _trackedRoots.Pop();
            if (_ccSession.ClusterTime != null)
                _baseSession.AdvanceClusterTime(_ccSession.ClusterTime);
            if (_ccSession.OperationTime != null)
                _baseSession.AdvanceOperationTime(_ccSession.OperationTime);

            _ccSession.Dispose();
            _ccSession = null;
        }
    }

    private async Task DispatchDomainEventsAndPopLastFrame()
    {
        var listRoots = _trackedRoots.Pop();
        HashSet<object> alreadyDispatched = new HashSet<object>(ReferenceEqualityComparer.Instance);
        foreach (var root in listRoots)
        {
            if (!alreadyDispatched.Contains(root))
            {
                var events = root.DomainEvents.ToList();
                root.ClearDomainEvents();
                alreadyDispatched.Add(root);
                foreach (var evt in events)
                {
                    //if (!IsInTransaction)
                    //    throw new InvalidOperationException("dispatching domain events needs a transaction");
                    await _mediator.Publish(evt);
                }
            }
        }
    }

    public void TrackAggregateRoot(IAggregateRoot root)
    {
        if (_baseSession == null)
        {
            _baseSession = Client.StartSession();
            _trackedRoots.Push(new List<IAggregateRoot>());
        }

        _trackedRoots.Peek().Add(root);
    }

    public async Task<TResult> OpenCausallyConsistentTransactionAsync<TResult>(Func<CancellationToken, Task<TResult>> action, string? consistencyToken = null, IsolationLevel? level = null, CancellationToken ct = default)
    {
        return await StartCausallyConsistentSectionAsync(async ct1 =>
        {
            return await OpenTransactionAsync(async ct2 =>
            {
                return await action(ct2);
            }, level, ct1);
        }, consistencyToken, ct);
    }

    public async Task<TResult> OpenTransactionAsync<TResult>(Func<CancellationToken, Task<TResult>> action, IsolationLevel? level = null, CancellationToken ct = default)
    {
        if (IsInTransaction)
            throw new InvalidOperationException("in transaction");

        if (_baseSession == null)
        {
            _baseSession = await Client.StartSessionAsync(cancellationToken: ct);
            _trackedRoots.Push(new List<IAggregateRoot>());
        }

        bool dispose = false;
        if (_ccSession == null)
        {
            dispose = true;  
            _ccSession = await Client.StartSessionAsync(cancellationToken: ct);
        }
        if (dispose)
        {
            if (_baseSession.ClusterTime != null)
                _ccSession.AdvanceClusterTime(_baseSession.ClusterTime);
            if (_baseSession.OperationTime != null)
                _ccSession.AdvanceOperationTime(_baseSession.OperationTime);
        }

        try
        {
            return await _ccSession.WithTransactionAsync(async (ss2, ct2) =>
            {
                _trackedRoots.Push(new List<IAggregateRoot>());
                bool popped = false;
                this._tranSession = ss2;
                try
                {

                    var r = await action(ct2);
                    popped = true;
                    await DispatchDomainEventsAndPopLastFrame();
                    return r;
                }
                finally
                {
                    if (!popped)
                        _trackedRoots.Pop();

                    if (!object.ReferenceEquals(_tranSession, _ccSession))
                    {
                        if (_tranSession.ClusterTime != null)
                            _ccSession.AdvanceClusterTime(_tranSession.ClusterTime);
                        if (_tranSession.OperationTime != null)
                            _ccSession.AdvanceOperationTime(_tranSession.OperationTime);
                    }
                    this._tranSession = null;
                }
            }, GetTransactionOptions(level), ct);
        }
        finally
        {
            if (dispose)
            {
                if (_ccSession.ClusterTime != null)
                    _baseSession.AdvanceClusterTime(_ccSession.ClusterTime);
                if (_ccSession.OperationTime != null)
                    _baseSession.AdvanceOperationTime(_ccSession.OperationTime);

                _ccSession.Dispose();
                _ccSession = null;
            }
        }
    }

    private TransactionOptions GetTransactionOptions(IsolationLevel? level)
    {
        if (level != null)
            return new TransactionOptions(GetReadConcern(level.Value), GetReadPreference(level.Value), GetWriteConcern(level.Value));

        if (DefaultIsolationLevel != null)
            return new TransactionOptions(GetReadConcern(DefaultIsolationLevel.Value), GetReadPreference(DefaultIsolationLevel.Value), GetWriteConcern(DefaultIsolationLevel.Value));

        return new TransactionOptions(ReadConcern.Majority, ReadPreference.Primary, WriteConcern.WMajority);
    }

    public async Task<TResult> RetryOnExceptions<TResult>(Func<CancellationToken, Task<TResult>> action, IEnumerable<Type> exceptionTypes, int retries = 1, CancellationToken ct = default)
    {
        //if (IsCausalllyConsistent)
        //    throw new InvalidOperationException("already causally consistent");
        //if (IsInTransaction)
        //    throw new InvalidOperationException("in transaction");

        var retryPolicy = Policy
                    .Handle<Exception>(e => exceptionTypes.Any(eb => eb.IsAssignableFrom(e.GetType())))
                    .WaitAndRetryAsync(retries, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt) * 0.5 + Math.Pow(2, retryAttempt) * Random.Shared.NextDouble()));

        return await retryPolicy.ExecuteAsync(async ct2 =>
        {
            _trackedRoots.Push(new List<IAggregateRoot>());
            bool popped = false;
            try
            {

                var r = await action(ct2);
                popped = true;
                await DispatchDomainEventsAndPopLastFrame();
                return r;

            }
            finally
            {
                if (!popped)
                    _trackedRoots.Pop();
            }
        }, ct);
    }

    public async Task<TResult> WithIsolationLevelAsync<TResult>(Func<CancellationToken, Task<TResult>> action, IsolationLevel level, CancellationToken ct = default)
    {
        _isolationLevelStack.Push(level);
        _trackedRoots.Push(new List<IAggregateRoot>());
        bool popped = false;
        try
        {
            var r = await action(ct);
            popped = true;
            await DispatchDomainEventsAndPopLastFrame();
            return r;
        }
        finally
        {
            if (!popped)
                _trackedRoots.Pop();
            _isolationLevelStack.Pop();
        }
    }

    private ReadConcern GetReadConcern(IsolationLevel level)
    {
        switch (level)
        {
            case IsolationLevel.Uncommitted:
                return ReadConcern.Local;
            case IsolationLevel.UncommittedStale:
                return ReadConcern.Local;
            case IsolationLevel.UncommittedNearest:
                return ReadConcern.Local;
            case IsolationLevel.Committed:
                return ReadConcern.Majority;
            case IsolationLevel.CommittedStale:
                return ReadConcern.Majority;
            case IsolationLevel.CommittedNearest:
                return ReadConcern.Majority;
            case IsolationLevel.Linearizable:
                return ReadConcern.Linearizable;
            case IsolationLevel.Snapshot:
                return ReadConcern.Snapshot;
            default:
                throw new InvalidOperationException();
        }
    }

    private WriteConcern GetWriteConcern(IsolationLevel level)
    {
        return WriteConcern.WMajority;
    }

    private ReadPreference GetReadPreference(IsolationLevel level)
    {
        switch (level)
        {
            case IsolationLevel.Uncommitted:
                return ReadPreference.Primary;
            case IsolationLevel.UncommittedStale:
                return ReadPreference.PrimaryPreferred;
            case IsolationLevel.UncommittedNearest:
                return ReadPreference.Nearest;
            case IsolationLevel.Committed:
                return ReadPreference.Primary;
            case IsolationLevel.CommittedStale:
                return ReadPreference.PrimaryPreferred;
            case IsolationLevel.CommittedNearest:
                return ReadPreference.Nearest;
            case IsolationLevel.Linearizable:
                return ReadPreference.Primary;
            case IsolationLevel.Snapshot:
                return ReadPreference.Primary;
            default:
                throw new InvalidOperationException();
        }
    }

    public void Dispose()
    {
        if (_tranSession != null)
        {
            _tranSession.Dispose();
            _tranSession = null;
        }

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

    public Type GetDuplicatedKeyExceptionType()
    {
        return typeof(MongoDuplicateKeyException);
    }

    public async Task<TResult> TrackAggregateRoots<TResult>(Func<CancellationToken, Task<TResult>> action, CancellationToken ct = default)
    {
        _trackedRoots.Push(new List<IAggregateRoot>());
        bool popped = false;
        try
        {
            var r = await action(ct);
            popped = true;
            await DispatchDomainEventsAndPopLastFrame();
            return r;
        }
        finally
        {
            if (!popped)
                _trackedRoots.Pop();
        }
    }
}
