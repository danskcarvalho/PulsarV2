namespace Pulsar.BuildingBlocks.UnitTests.Mocking.MongoDB;

public interface IMockedCollection<T> where T : class
{
    Task InsertManyAsync(IEnumerable<T> items);
    Task<T?> FindByIdAsync(ObjectId id);
    IAsyncEnumerable<T> FindAsync(IFindSpecification<T> specification);
    IAsyncEnumerable<TProjection> FindAsync<TProjection>(IFindSpecification<T, TProjection> specification);
    Task<long> DeleteManyAsync(IDeleteSpecification<T> specification, long? limit = null);
    Task<long> UpdateManyAsync(IUpdateSpecification<T> specification, long? limit = null);
    Task<bool> ExistsAsync(ObjectId id);
    Task<long> ReplaceAsync(T item, ObjectId id, long? version = null, string? propertyName = null);
    Task AddUniqueKey<TKey>(Func<T, TKey> keyExtractor) where TKey : notnull;
}
