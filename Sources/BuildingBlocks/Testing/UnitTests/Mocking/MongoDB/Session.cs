namespace Pulsar.BuildingBlocks.UnitTests.Mocking.MongoDB;

class Session : IMockedDbSession, IDbSession
{
    public IMockedDatabase Database { get; private set; }
    private IMediator _mediator;
    private Stack<List<IAggregateRoot>> _trackedRoots = new Stack<List<IAggregateRoot>>();

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

        _trackedRoots.Push(new List<IAggregateRoot>());
        bool popped = false;
        try
        {
            var r = await action(ct);
            popped = await DispatchDomainEventsAndPopLastFrame(r);
            return r;
        }
        finally
        {
            if (!popped)
                _trackedRoots.Pop();
        }
    }

    private async Task<bool> DispatchDomainEventsAndPopLastFrame(object? result)
    {
        while (_trackedRoots.Peek().Count != 0)
        {
            var events = new List<INotification>();

            foreach (var root in _trackedRoots.Peek())
            {
                events.AddRange(root.DomainEvents);
                root.ClearDomainEvents();
            }


            foreach (var evt in events)
            {
                //if (!IsInTransaction)
                //    throw new InvalidOperationException("dispatching domain events needs a transaction");
                await _mediator.Publish(evt);
            }

            _trackedRoots.Peek().RemoveAll(tr => tr.DomainEvents.Count == 0); //clear roots without events from current frame
        }

        var prop = result?.GetType().GetProperty("ConsistencyToken");
        if (prop != null && prop.CanWrite)
            prop.SetValue(result, this.ConsistencyToken);
        _trackedRoots.Pop();
        return true;
    }

    public void TrackAggregateRoot(IAggregateRoot root)
    {
        if (_trackedRoots.Count == 0)
            throw new InvalidOperationException("no tracking frame pushed into the stack");

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

        return await Database.WithTransactionAsync(async () =>
        {
            _trackedRoots.Push(new List<IAggregateRoot>());
            bool popped = false;
            try
            {

                var r = await action(ct);
                popped = await DispatchDomainEventsAndPopLastFrame(r);
                return r;
            }
            finally
            {
                if (!popped)
                    _trackedRoots.Pop();
            }
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
            _trackedRoots.Push(new List<IAggregateRoot>());
            bool popped = false;
            try
            {

                var r = await action(ct2);
                popped = await DispatchDomainEventsAndPopLastFrame(r);
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
        _trackedRoots.Push(new List<IAggregateRoot>());
        bool popped = false;
        try
        {
            var r = await action(ct);
            popped = await DispatchDomainEventsAndPopLastFrame(r);
            return r;
        }
        finally
        {
            if (!popped)
                _trackedRoots.Pop();
        }
    }


    public void Dispose()
    {
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
            popped = await DispatchDomainEventsAndPopLastFrame(r);
            return r;
        }
        finally
        {
            if (!popped)
                _trackedRoots.Pop();
        }
    }
}
