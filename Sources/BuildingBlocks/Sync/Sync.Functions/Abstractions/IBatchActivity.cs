namespace Pulsar.BuildingBlocks.Sync.Functions.Abstractions;

public interface IBatchActivity
{
    Task Execute(BatchActivityDescription desc);
    Task<TResult> Execute<TResult>(BatchActivityDescription<TResult> desc);
}