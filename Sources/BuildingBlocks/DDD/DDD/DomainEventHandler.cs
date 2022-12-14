using MediatR;

namespace Pulsar.BuildingBlocks.DDD;

public abstract class DomainEventHandler<TEvent> : INotificationHandler<TEvent> where TEvent : INotification
{
    private IDbSession _session;
    public IDbSession Session => _session;

    protected DomainEventHandler(IDbSession session)
    {
        _session = session;
    }

    protected abstract Task HandleAsync(TEvent evt, CancellationToken ct);

    async Task INotificationHandler<TEvent>.Handle(TEvent notification, CancellationToken cancellationToken)
    {
        var retryOnExc = this.GetType().GetCustomAttributes(typeof(RetryOnExceptionAttribute), true).Cast<RetryOnExceptionAttribute>().FirstOrDefault();

        if (retryOnExc != null)
        {
            await _session.RetryOnExceptions(async ct2 =>
            {
                await HandleAsync(notification, ct2);
                return 0;
            }, GetExceptionTypes(retryOnExc), retryOnExc.Retries <= 0 ? 1 : retryOnExc.Retries, cancellationToken);
        }
        else
            await HandleAsync(notification, cancellationToken);
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
