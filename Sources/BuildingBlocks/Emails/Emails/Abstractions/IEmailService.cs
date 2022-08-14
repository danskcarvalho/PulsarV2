namespace Pulsar.BuildingBlocks.Emails.Abstractions;

public interface IEmailService
{
    Task Send<TModel>(TModel model, CancellationToken ct = default, bool throwOnError = false) where TModel : class;
}
