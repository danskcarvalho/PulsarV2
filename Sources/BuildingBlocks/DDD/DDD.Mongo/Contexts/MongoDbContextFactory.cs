using Pulsar.BuildingBlocks.DDD.Contexts;

namespace Pulsar.BuildingBlocks.DDD.Mongo.Contexts;

public class MongoDbContextFactory : IDbContextFactory
{
    private List<IIsRepository> _repositories;

    public MongoDbContextFactory(IEnumerable<IIsRepository> repositories)
    {
        _repositories = repositories.ToList();
    }

    public IDbContext CreateContext()
    {
        return new MongoDbContext(_repositories);
    }
}
