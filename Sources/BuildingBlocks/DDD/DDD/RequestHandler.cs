namespace Pulsar.BuildingBlocks.DDD;

public abstract class RequestHandler<TRequest> : IRequestHandler<TRequest> where TRequest : IRequest<Unit>
{
    private IDbSession _session;
    public IDbSession Session => _session;

    protected RequestHandler(IDbSession session)
    {
        _session = session;
    }

    protected abstract Task HandleAsync(TRequest request, CancellationToken ct);

    async Task<Unit> IRequestHandler<TRequest, Unit>.Handle(TRequest request, CancellationToken cancellationToken)
    {
        var requiresCC = this.GetType().GetCustomAttributes(typeof(RequiresCausalConsistencyAttribute), true).Cast<RequiresCausalConsistencyAttribute>().FirstOrDefault();
        var noTran = this.GetType().GetCustomAttributes(typeof(NoTransactionAttribute), true).Cast<NoTransactionAttribute>().FirstOrDefault();
        var retryOnExc = this.GetType().GetCustomAttributes(typeof(RetryOnExceptionExceptionAttribute), true).Cast<RetryOnExceptionExceptionAttribute>().FirstOrDefault();
        var withIso = this.GetType().GetCustomAttributes(typeof(WithIsolationLevelAttribute), true).Cast<WithIsolationLevelAttribute>().FirstOrDefault();

        if (withIso != null && noTran != null)
        {
            await _session.WithIsolationLevelAsync(async (ct1) =>
            {
                await RetryOnException(request, requiresCC, noTran, retryOnExc, withIso, ct1);
                return 0;
            }, withIso.IsolationLevel, cancellationToken);
        }
        else
            await RetryOnException(request, requiresCC, noTran, retryOnExc, withIso, cancellationToken);

        return Unit.Value;
    }

    private async Task RetryOnException(TRequest request, RequiresCausalConsistencyAttribute? requiresCC, NoTransactionAttribute? noTran, RetryOnExceptionExceptionAttribute? retryOnExc, WithIsolationLevelAttribute? withIso, CancellationToken ct1)
    {
        if (retryOnExc != null)
        {
            await _session.RetryOnExceptions(async ct2 =>
            {
                await CreateTransactionOrCCSesction(request, requiresCC, noTran, withIso, ct2);
                return 0;
            }, GetExceptionTypes(retryOnExc), retryOnExc.Retries ?? 1, ct1);
        }
        else
            await CreateTransactionOrCCSesction(request, requiresCC, noTran, withIso, ct1);
    }

    private IEnumerable<Type> GetExceptionTypes(RetryOnExceptionExceptionAttribute retryOnExc)
    {
        if (retryOnExc.DuplicatedKey)
            yield return Session.GetDuplicatedKeyExceptionType();
        if (retryOnExc.VersionConcurrency)
            yield return typeof(VersionConcurrencyException);

        foreach (var et in retryOnExc.ExceptionTypes)
        {
            yield return et;
        }
    }

    private async Task CreateTransactionOrCCSesction(TRequest request, RequiresCausalConsistencyAttribute? requiresCC, NoTransactionAttribute? noTran, WithIsolationLevelAttribute? withIso, CancellationToken ct)
    {
        if (noTran == null && requiresCC != null)
        {
            var token = requiresCC.CasualConsistencyTokenProperty != null ? typeof(TRequest).GetProperty(requiresCC.CasualConsistencyTokenProperty)?.GetValue(request) as string : null;

            if (token != null || !requiresCC.IfTokenIsPresent)
            {
                await _session.OpenCausallyConsistentTransactionAsync(async ct1 =>
                {
                    await HandleAsync(request, ct1);
                    return 0;
                },
                token,
                withIso?.IsolationLevel ?? IsolationLevel.Committed,
                ct);
            }
            else
            {
                await _session.OpenTransactionAsync(async ct1 =>
                {
                    await HandleAsync(request, ct1);
                    return 0;
                },
                withIso?.IsolationLevel ?? IsolationLevel.Committed,
                ct);
            }

        }
        else if (noTran == null)
        {
            await _session.OpenTransactionAsync(async ct1 =>
            {
                await HandleAsync(request, ct1);
                return 0;
            },
            withIso?.IsolationLevel ?? IsolationLevel.Committed,
            ct);
        }
        else if (requiresCC != null)
        {
            var token = requiresCC.CasualConsistencyTokenProperty != null ? typeof(TRequest).GetProperty(requiresCC.CasualConsistencyTokenProperty)?.GetValue(request) as string : null;

            if (token != null || !requiresCC.IfTokenIsPresent)
            {
                await _session.StartCausallyConsistentSectionAsync(async ct1 =>
                {
                    await HandleAsync(request, ct1);
                    return 0;
                },
                token,
                ct);
            }
            else
            {
                await HandleAsync(request, ct);
            }
        }
        else
            await HandleAsync(request, ct);
    }
}

public abstract class RequestHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private IDbSession _session;
    public IDbSession Session => _session;

    protected RequestHandler(IDbSession session)
    {
        _session = session;
    }

    protected abstract Task<TResponse> HandleAsync(TRequest request, CancellationToken ct);

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
    {
        var requiresCC = this.GetType().GetCustomAttributes(typeof(RequiresCausalConsistencyAttribute), true).Cast<RequiresCausalConsistencyAttribute>().FirstOrDefault();
        var noTran = this.GetType().GetCustomAttributes(typeof(NoTransactionAttribute), true).Cast<NoTransactionAttribute>().FirstOrDefault();
        var retryOnExc = this.GetType().GetCustomAttributes(typeof(RetryOnExceptionExceptionAttribute), true).Cast<RetryOnExceptionExceptionAttribute>().FirstOrDefault();
        var withIso = this.GetType().GetCustomAttributes(typeof(WithIsolationLevelAttribute), true).Cast<WithIsolationLevelAttribute>().FirstOrDefault();

        if (withIso != null)
        {
            return await _session.WithIsolationLevelAsync(async (ct1) =>
            {
                return await RetryOnException(request, requiresCC, noTran, retryOnExc, withIso, ct1);
            }, withIso.IsolationLevel, cancellationToken);
        }
        else
            return await RetryOnException(request, requiresCC, noTran, retryOnExc, withIso, cancellationToken);
    }

    private async Task<TResponse> RetryOnException(TRequest request, RequiresCausalConsistencyAttribute? requiresCC, NoTransactionAttribute? requiresTran, RetryOnExceptionExceptionAttribute? retryOnExc, WithIsolationLevelAttribute? withIso, CancellationToken ct1)
    {
        if (retryOnExc != null)
        {
            return await _session.RetryOnExceptions(async ct2 =>
            {
                return await CreateTransactionOrCCSesction(request, requiresCC, requiresTran, withIso, ct2);
            }, GetExceptionTypes(retryOnExc), retryOnExc.Retries ?? 1, ct1);
        }
        else
            return await CreateTransactionOrCCSesction(request, requiresCC, requiresTran, withIso, ct1);
    }

    private IEnumerable<Type> GetExceptionTypes(RetryOnExceptionExceptionAttribute retryOnExc)
    {
        if (retryOnExc.DuplicatedKey)
            yield return Session.GetDuplicatedKeyExceptionType();
        if (retryOnExc.VersionConcurrency)
            yield return typeof(VersionConcurrencyException);

        foreach (var et in retryOnExc.ExceptionTypes)
        {
            yield return et;
        }
    }

    private async Task<TResponse> CreateTransactionOrCCSesction(TRequest request, RequiresCausalConsistencyAttribute? requiresCC, NoTransactionAttribute? noTran, WithIsolationLevelAttribute? withIso, CancellationToken ct)
    {
        if (noTran == null && requiresCC != null)
        {
            var token = requiresCC.CasualConsistencyTokenProperty != null ? typeof(TRequest).GetProperty(requiresCC.CasualConsistencyTokenProperty)?.GetValue(request) as string : null;

            if (token != null || !requiresCC.IfTokenIsPresent)
            {
                return await _session.OpenCausallyConsistentTransactionAsync(async ct1 =>
                {
                    return await HandleAsync(request, ct1);
                },
                token,
                withIso?.IsolationLevel ?? IsolationLevel.Committed,
                ct);
            }
            else
            {
                return await _session.OpenTransactionAsync(async ct1 =>
                {
                    return await HandleAsync(request, ct1);
                },
                withIso?.IsolationLevel ?? IsolationLevel.Committed,
                ct);
            }

        }
        else if (noTran == null)
        {
            return await _session.OpenTransactionAsync(async ct1 =>
            {
                return await HandleAsync(request, ct1);
            },
            withIso?.IsolationLevel ?? IsolationLevel.Committed,
            ct);
        }
        else if (requiresCC != null)
        {
            var token = requiresCC.CasualConsistencyTokenProperty != null ? typeof(TRequest).GetProperty(requiresCC.CasualConsistencyTokenProperty)?.GetValue(request) as string : null;

            if (token != null || !requiresCC.IfTokenIsPresent)
            {
                return await _session.StartCausallyConsistentSectionAsync(async ct1 =>
                {
                    return await HandleAsync(request, ct1);
                },
                token,
                ct);
            }
            else
            {
                return await HandleAsync(request, ct);
            }
        }
        else
            return await HandleAsync(request, ct);
    }
}
