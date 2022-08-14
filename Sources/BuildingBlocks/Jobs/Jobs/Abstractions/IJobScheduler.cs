namespace Pulsar.BuildingBlocks.Jobs.Abstractions;

public interface IJobScheduler
{
    Task Enqueue<TService>(Expression<Func<TService, Task>> methodCall);
}
