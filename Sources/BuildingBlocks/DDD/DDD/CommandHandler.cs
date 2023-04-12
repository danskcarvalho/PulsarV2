namespace Pulsar.BuildingBlocks.DDD;

public abstract class CommandHandler<TCommand> : IRequestHandler<TCommand> where TCommand : IRequest
{
    private IDbSession _session;
    public IDbSession Session => _session;

    protected CommandHandler(IDbSession session)
    {
        _session = session;
    }

    protected abstract Task HandleAsync(TCommand cmd, CancellationToken ct);

    async Task IRequestHandler<TCommand>.Handle(TCommand request, CancellationToken cancellationToken)
    {
        var requiresCC = this.GetType().GetCustomAttributes(typeof(RequiresCausalConsistencyAttribute), true).Cast<RequiresCausalConsistencyAttribute>().FirstOrDefault();
        var noTran = this.GetType().GetCustomAttributes(typeof(NoTransactionAttribute), true).Cast<NoTransactionAttribute>().FirstOrDefault();
        var retryOnExc = this.GetType().GetCustomAttributes(typeof(RetryOnExceptionAttribute), true).Cast<RetryOnExceptionAttribute>().FirstOrDefault();
        var withIso = this.GetType().GetCustomAttributes(typeof(WithIsolationLevelAttribute), true).Cast<WithIsolationLevelAttribute>().FirstOrDefault();

        await _session.TrackAggregateRoots(async ct =>
        {
            if (withIso != null && noTran != null)
            {
                await _session.WithIsolationLevelAsync(async (ct1) =>
                {
                    await RetryOnException(request, requiresCC, noTran, retryOnExc, withIso, ct1);
                    return 0;
                }, withIso.IsolationLevel, ct);
            }
            else
                await RetryOnException(request, requiresCC, noTran, retryOnExc, withIso, ct);

            return Unit.Value;
        }, cancellationToken);
    }

    private async Task RetryOnException(TCommand request, RequiresCausalConsistencyAttribute? requiresCC, NoTransactionAttribute? noTran, RetryOnExceptionAttribute? retryOnExc, WithIsolationLevelAttribute? withIso, CancellationToken ct1)
    {
        if (retryOnExc != null)
        {
            await _session.RetryOnExceptions(async ct2 =>
            {
                await CreateTransactionOrCCSesction(request, requiresCC, noTran, withIso, ct2);
                return 0;
            }, GetExceptionTypes(retryOnExc), retryOnExc.Retries <= 0 ? 1 : retryOnExc.Retries, ct1);
        }
        else
            await CreateTransactionOrCCSesction(request, requiresCC, noTran, withIso, ct1);
    }

    private IEnumerable<Type> GetExceptionTypes(RetryOnExceptionAttribute retryOnExc)
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

    private async Task CreateTransactionOrCCSesction(TCommand request, RequiresCausalConsistencyAttribute? requiresCC, NoTransactionAttribute? noTran, WithIsolationLevelAttribute? withIso, CancellationToken ct)
    {
        if (noTran == null && requiresCC != null)
        {
            var token = requiresCC.CasualConsistencyTokenProperty != null ? typeof(TCommand).GetProperty(requiresCC.CasualConsistencyTokenProperty)?.GetValue(request) as string : null;

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
            var token = requiresCC.CasualConsistencyTokenProperty != null ? typeof(TCommand).GetProperty(requiresCC.CasualConsistencyTokenProperty)?.GetValue(request) as string : null;

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

public abstract class CommandHandler<TCommand, TResult> : IRequestHandler<TCommand, TResult> where TCommand : IRequest<TResult>
{
    private IDbSession _session;
    public IDbSession Session => _session;

    protected CommandHandler(IDbSession session)
    {
        _session = session;
    }

    protected abstract Task<TResult> HandleAsync(TCommand cmd, CancellationToken ct);

    public async Task<TResult> Handle(TCommand request, CancellationToken cancellationToken)
    {
        var requiresCC = this.GetType().GetCustomAttributes(typeof(RequiresCausalConsistencyAttribute), true).Cast<RequiresCausalConsistencyAttribute>().FirstOrDefault();
        var noTran = this.GetType().GetCustomAttributes(typeof(NoTransactionAttribute), true).Cast<NoTransactionAttribute>().FirstOrDefault();
        var retryOnExc = this.GetType().GetCustomAttributes(typeof(RetryOnExceptionAttribute), true).Cast<RetryOnExceptionAttribute>().FirstOrDefault();
        var withIso = this.GetType().GetCustomAttributes(typeof(WithIsolationLevelAttribute), true).Cast<WithIsolationLevelAttribute>().FirstOrDefault();

        return await _session.TrackAggregateRoots(async ct =>
        {
            if (withIso != null)
            {
                return await _session.WithIsolationLevelAsync(async (ct1) =>
                {
                    return await RetryOnException(request, requiresCC, noTran, retryOnExc, withIso, ct1);
                }, withIso.IsolationLevel, ct);
            }
            else
                return await RetryOnException(request, requiresCC, noTran, retryOnExc, withIso, ct);
        }, cancellationToken);
    }

    private async Task<TResult> RetryOnException(TCommand request, RequiresCausalConsistencyAttribute? requiresCC, NoTransactionAttribute? requiresTran, RetryOnExceptionAttribute? retryOnExc, WithIsolationLevelAttribute? withIso, CancellationToken ct1)
    {
        if (retryOnExc != null)
        {
            return await _session.RetryOnExceptions(async ct2 =>
            {
                return await CreateTransactionOrCCSesction(request, requiresCC, requiresTran, withIso, ct2);
            }, GetExceptionTypes(retryOnExc), retryOnExc.Retries <= 0 ? 1 : retryOnExc.Retries, ct1);
        }
        else
            return await CreateTransactionOrCCSesction(request, requiresCC, requiresTran, withIso, ct1);
    }

    private IEnumerable<Type> GetExceptionTypes(RetryOnExceptionAttribute retryOnExc)
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

    private async Task<TResult> CreateTransactionOrCCSesction(TCommand request, RequiresCausalConsistencyAttribute? requiresCC, NoTransactionAttribute? noTran, WithIsolationLevelAttribute? withIso, CancellationToken ct)
    {
        if (noTran == null && requiresCC != null)
        {
            var token = requiresCC.CasualConsistencyTokenProperty != null ? typeof(TCommand).GetProperty(requiresCC.CasualConsistencyTokenProperty)?.GetValue(request) as string : null;

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
            var token = requiresCC.CasualConsistencyTokenProperty != null ? typeof(TCommand).GetProperty(requiresCC.CasualConsistencyTokenProperty)?.GetValue(request) as string : null;

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
