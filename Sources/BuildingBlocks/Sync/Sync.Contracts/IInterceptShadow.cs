namespace Pulsar.BuildingBlocks.Sync.Contracts;

public interface IInterceptShadow<TShadow> where TShadow : class, IShadow
{
    void Intercept(TShadow shadow);
}
