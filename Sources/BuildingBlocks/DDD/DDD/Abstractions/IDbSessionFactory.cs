namespace Pulsar.BuildingBlocks.DDD.Abstractions;

public interface IDbSessionFactory
{
    IDbSession CreateSession();
}
