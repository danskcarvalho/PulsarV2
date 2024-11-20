using Pulsar.BuildingBlocks.DDD.Contexts;

namespace Pulsar.BuildingBlocks.DDD;

public abstract class NotificationHandler<TNotification> : INotificationHandler<TNotification> where TNotification : INotification
{
    private DbContextFactory _contextFactory;
    private IDbSession _session;
    public IDbSession Session => _session;

    protected NotificationHandler(IDbSession session, DbContextFactory contextFactory)
    {
        _session = session;
        _contextFactory = contextFactory;
    }

    protected abstract Task HandleAsync(TNotification cmd, CancellationToken ct);

    async Task INotificationHandler<TNotification>.Handle(TNotification request, CancellationToken cancellationToken)
    {
        var requiresCC = this.GetType().GetCustomAttributes(typeof(RequiresCausalConsistencyAttribute), true).Cast<RequiresCausalConsistencyAttribute>().FirstOrDefault();
        var noTran = this.GetType().GetCustomAttributes(typeof(NoTransactionAttribute), true).Cast<NoTransactionAttribute>().FirstOrDefault();
        var retryOnExc = this.GetType().GetCustomAttributes(typeof(RetryOnExceptionAttribute), true).Cast<RetryOnExceptionAttribute>().FirstOrDefault();
        var withIso = this.GetType().GetCustomAttributes(typeof(WithIsolationLevelAttribute), true).Cast<WithIsolationLevelAttribute>().FirstOrDefault();

        await _session.TrackConsistencyToken(async ct =>
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

    private async Task RetryOnException(TNotification request, RequiresCausalConsistencyAttribute? requiresCC, NoTransactionAttribute? noTran, RetryOnExceptionAttribute? retryOnExc, WithIsolationLevelAttribute? withIso, CancellationToken ct1)
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

    private async Task CreateTransactionOrCCSesction(TNotification request, RequiresCausalConsistencyAttribute? requiresCC, NoTransactionAttribute? noTran, WithIsolationLevelAttribute? withIso, CancellationToken ct)
    {
        if (noTran == null && requiresCC != null)
        {
            var token = requiresCC.CasualConsistencyTokenProperty != null ? typeof(TNotification).GetProperty(requiresCC.CasualConsistencyTokenProperty)?.GetValue(request) as string : null;

            if (token != null || !requiresCC.IfTokenIsPresent)
            {
                await _session.OpenCausallyConsistentTransactionAsync(async ct1 =>
                {
                    await CallHandler(request, ct1);
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
                    await CallHandler(request, ct1);
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
                await CallHandler(request, ct1);
                return 0;
            },
            withIso?.IsolationLevel ?? IsolationLevel.Committed,
            ct);
        }
        else if (requiresCC != null)
        {
            var token = requiresCC.CasualConsistencyTokenProperty != null ? typeof(TNotification).GetProperty(requiresCC.CasualConsistencyTokenProperty)?.GetValue(request) as string : null;

            if (token != null || !requiresCC.IfTokenIsPresent)
            {
                await _session.StartCausallyConsistentSectionAsync(async ct1 =>
                {
                    await CallHandler(request, ct1);
                    return 0;
                },
                token,
                ct);
            }
            else
            {
                await CallHandler(request, ct);
            }
        }
        else
            await CallHandler(request, ct);
    }

    private async Task CallHandler(TNotification request, CancellationToken ct)
    {
        var ctx = _contextFactory.CreateContext();
        DbContextImpl.SetContext(ctx);
        try
        {
            await HandleAsync(request, ct);
        }
        finally
        {
            DbContextImpl.ClearContext();
        }
    }
}
