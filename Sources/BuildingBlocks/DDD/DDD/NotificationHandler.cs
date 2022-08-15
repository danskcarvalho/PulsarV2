namespace Pulsar.BuildingBlocks.DDD;

public abstract class NotificationHandler<TNofication> : INotificationHandler<TNofication> where TNofication : INotification
{
    private IDbSession _session;
    public IDbSession Session => _session;

    protected NotificationHandler(IDbSession session)
    {
        _session = session;
    }

    protected abstract Task HandleAsync(TNofication request, CancellationToken ct);

    async Task INotificationHandler<TNofication>.Handle(TNofication notification, CancellationToken cancellationToken)
    {
        var requiresCC = this.GetType().GetCustomAttributes(typeof(RequiresCausalConsistencyAttribute), true).Cast<RequiresCausalConsistencyAttribute>().FirstOrDefault();
        var noTran = this.GetType().GetCustomAttributes(typeof(NoTransactionAttribute), true).Cast<NoTransactionAttribute>().FirstOrDefault();
        var retryOnExc = this.GetType().GetCustomAttributes(typeof(RetryOnExceptionAttribute), true).Cast<RetryOnExceptionAttribute>().FirstOrDefault();
        var withIso = this.GetType().GetCustomAttributes(typeof(WithIsolationLevelAttribute), true).Cast<WithIsolationLevelAttribute>().FirstOrDefault();

        if (withIso != null && noTran != null)
        {
            await _session.WithIsolationLevelAsync(async (ct1) =>
            {
                await RetryOnException(notification, requiresCC, noTran, retryOnExc, withIso, ct1);
                return 0;
            }, withIso.IsolationLevel, cancellationToken);
        }
        else
            await RetryOnException(notification, requiresCC, noTran, retryOnExc, withIso, cancellationToken);
    }

    private async Task RetryOnException(TNofication request, RequiresCausalConsistencyAttribute? requiresCC, NoTransactionAttribute? noTran, RetryOnExceptionAttribute? retryOnExc, WithIsolationLevelAttribute? withIso, CancellationToken ct1)
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

    private async Task CreateTransactionOrCCSesction(TNofication request, RequiresCausalConsistencyAttribute? requiresCC, NoTransactionAttribute? noTran, WithIsolationLevelAttribute? withIso, CancellationToken ct)
    {
        if (noTran == null && requiresCC != null)
        {
            var token = requiresCC.CasualConsistencyTokenProperty != null ? typeof(TNofication).GetProperty(requiresCC.CasualConsistencyTokenProperty)?.GetValue(request) as string : null;

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
            var token = requiresCC.CasualConsistencyTokenProperty != null ? typeof(TNofication).GetProperty(requiresCC.CasualConsistencyTokenProperty)?.GetValue(request) as string : null;

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
