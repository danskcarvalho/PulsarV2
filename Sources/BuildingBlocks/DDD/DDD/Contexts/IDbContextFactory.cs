namespace Pulsar.BuildingBlocks.DDD.Contexts;

public interface IDbContextFactory
{
    IDbContext CreateContext();
}
