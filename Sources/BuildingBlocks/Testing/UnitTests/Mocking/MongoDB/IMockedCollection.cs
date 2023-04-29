namespace Pulsar.BuildingBlocks.UnitTests.Mocking.MongoDB;

public interface IMockedCollection<T> where T : class
{
    Task InsertManyAsync(IEnumerable<T> items);
    Task<T?> FindByIdAsync(ObjectId id);
    IAsyncEnumerable<T> FindAsync(IFindSpecification<T> specification);
    IAsyncEnumerable<TProjection> FindAsync<TProjection>(IFindSpecification<T, TProjection> specification);
    Task<int> DeleteManyAsync(IDeleteSpecification<T> specification, int? limit = null);
    Task<int> UpdateManyAsync(IUpdateSpecification<T> specification, int? limit = null);
    Task<bool> ExistsAsync(ObjectId id);
    Task<int> ReplaceAsync(T item, ObjectId id, long? version = null, string? propertyName = null);
    Task AddUniqueKey<TKey>(Func<T, TKey> keyExtractor) where TKey : notnull;
}
