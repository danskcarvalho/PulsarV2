using MediatR;
using Pulsar.BuildingBlocks.DDD.Contexts;

namespace Pulsar.BuildingBlocks.DDD;

public abstract class DomainEventHandler<TEvent> : INotificationHandler<TEvent> where TEvent : INotification
{
    private IDbContextFactory _contextFactory;
    private IDbSession _session;
    public IDbSession Session => _session;

    protected DomainEventHandler(IDbSession session, IDbContextFactory contextFactory)
    {
        _session = session;
        _contextFactory = contextFactory;
    }

    protected abstract Task HandleAsync(TEvent evt, CancellationToken ct);

    async Task INotificationHandler<TEvent>.Handle(TEvent notification, CancellationToken cancellationToken)
    {
        var retryOnExc = this.GetType().GetCustomAttributes(typeof(RetryOnExceptionAttribute), true).Cast<RetryOnExceptionAttribute>().FirstOrDefault();

        if (retryOnExc != null)
        {
            await _session.RetryOnExceptions(async ct2 =>
            {
                await CallHandler(notification, ct2);
                return 0;
            }, GetExceptionTypes(retryOnExc), retryOnExc.Retries <= 0 ? 1 : retryOnExc.Retries, cancellationToken);
        }
        else
            await CallHandler(notification, cancellationToken);
    }

    private async Task CallHandler(TEvent notification, CancellationToken cancellationToken)
    {
        var ctx = _contextFactory.CreateContext();
        DbContext.SetContext(ctx);
        try
        {
            await HandleAsync(notification, cancellationToken);
        }
        finally
        {
            DbContext.ClearContext();
        }
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
}
