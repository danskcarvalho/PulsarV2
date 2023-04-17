namespace Pulsar.BuildingBlocks.Emails.Abstractions;

public interface IEmailService : IAsyncDisposable
{
    Task Send<TModel>(TModel model, CancellationToken ct = default, bool throwOnError = false) where TModel : class;
}
