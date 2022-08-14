namespace Pulsar.BuildingBlocks.DDD.Abstractions;

public interface IDbSession : IAsyncDisposable
{
    bool IsCausalllyConsistent { get; }
    bool IsInTransaction { get; }
    string? ConsistencyToken { get; }
    public IsolationLevel? DefaultIsolationLevel { get; }

    Task<TResult> StartCausallyConsistentSectionAsync<TResult>(Func<CancellationToken, Task<TResult>> action, string? consistencyToken = null, CancellationToken ct = default);
    Task<TResult> OpenTransactionAsync<TResult>(Func<CancellationToken, Task<TResult>> action, IsolationLevel? level = null, CancellationToken ct = default);
    Task<TResult> OpenCausallyConsistentTransactionAsync<TResult>(Func<CancellationToken, Task<TResult>> action, string? consistencyToken = null, IsolationLevel? level = null, CancellationToken ct = default);
    Task<TResult> WithIsolationLevelAsync<TResult>(Func<CancellationToken, Task<TResult>> action, IsolationLevel level, CancellationToken ct = default);
    Task<TResult> RetryOnExceptions<TResult>(Func<CancellationToken, Task<TResult>> action, IEnumerable<Type> exceptionTypes, int retries = 1, CancellationToken ct = default);
    Type GetDuplicatedKeyExceptionType();
}
