namespace Pulsar.BuildingBlocks.UnitTests.Mocking.Emails;

public class MockedEmailService : IEmailService, IDisposable
{
    private readonly List<object> _emails = new List<object>();
    private readonly ReadOnlyCollection<object> _roList;

    public MockedEmailService()
    {
        _roList = _emails.AsReadOnly();
    }

    public IReadOnlyList<object> EmailsSent => _roList;

    public void Dispose()
    {
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public Task Send<TModel>(TModel model, CancellationToken ct = default, bool throwOnError = false) where TModel : class
    {
        lock (this)
        {
            _emails.Add(model);
            return Task.CompletedTask;
        }
    }
}
