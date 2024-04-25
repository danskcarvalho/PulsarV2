namespace Pulsar.BuildingBlocks.Sync.Contracts;

public interface IInterceptShadow<TShadow> where TShadow : class
{
    void Intercept(TShadow shadow);
}
