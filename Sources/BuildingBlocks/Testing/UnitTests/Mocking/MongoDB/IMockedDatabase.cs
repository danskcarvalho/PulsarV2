namespace Pulsar.BuildingBlocks.UnitTests.Mocking.MongoDB;

public interface IMockedDatabase
{
    bool IsInTransaction { get; }
    Task<TResult> WithTransactionAsync<TResult>(Func<Task<TResult>> action);
    IMockedCollection<T> GetCollection<T>(string collectionName) where T : class;
    IMockedDatabase Clone();
}
