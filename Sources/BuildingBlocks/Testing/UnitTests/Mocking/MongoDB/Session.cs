namespace Pulsar.BuildingBlocks.UnitTests.Mocking.MongoDB;

class Session : IMockedDbSession, IDbSession
{
    public IMockedDatabase Database { get; private set; }
    private IMediator _mediator;

    public Session(IMockedDatabase database, IMediator mediator)
    {
        Database = database;
        _mediator = mediator;
    }

    public bool IsCausalllyConsistent => false;

    public bool IsInTransaction => Database.IsInTransaction;

    public string? ConsistencyToken => null;

    public IsolationLevel? DefaultIsolationLevel => null;

    public async Task<TResult> StartCausallyConsistentSectionAsync<TResult>(Func<CancellationToken, Task<TResult>> action, string? consistencyToken = null, CancellationToken ct = default)
    {
        if (IsCausalllyConsistent)
            throw new InvalidOperationException("already causally consistent");
        if (IsInTransaction)
            throw new InvalidOperationException("in transaction");

        var r = await action(ct);
        SetConsistencyToken(r);
        return r;
    }

    public async Task DispatchDomainEvents(IAggregateRoot root)
    {
        var events = new List<INotification>();
        events.AddRange(root.DomainEvents);
        root.ClearDomainEvents();
        foreach (var evt in events)
        {
            //if (!IsInTransaction)
            //    throw new InvalidOperationException("dispatching domain events needs a transaction");
            await _mediator.Publish(evt);
        }
    }
    
    private void SetConsistencyToken(object? result)
    {
        var prop = result?.GetType().GetProperty("ConsistencyToken");
        if (prop != null && prop.CanWrite)
            prop.SetValue(result, this.ConsistencyToken);
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

        return await Database.WithTransactionAsync(async () =>
        {
            var r = await action(ct);
            SetConsistencyToken(r);
            return r;
        });
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
            var r = await action(ct2);
            SetConsistencyToken(r);
            return r;
        }, ct);
    }

    public async Task<TResult> WithIsolationLevelAsync<TResult>(Func<CancellationToken, Task<TResult>> action, IsolationLevel level, CancellationToken ct = default)
    {
        var r = await action(ct);
        SetConsistencyToken(r);
        return r;
    }


    public void Dispose()
    {
    }

    public Type GetDuplicatedKeyExceptionType()
    {
        return typeof(MongoDuplicateKeyException);
    }

    public async Task<TResult> TrackConsistencyToken<TResult>(Func<CancellationToken, Task<TResult>> action, CancellationToken ct = default)
    {
        var r = await action(ct);
        SetConsistencyToken(r);
        return r;
    }
}
