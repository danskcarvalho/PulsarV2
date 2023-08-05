namespace Pulsar.BuildingBlocks.DDD.Contexts;

public class DbContextFactory
{
    private List<IIsRepository> _repositories;

    public DbContextFactory(IEnumerable<IIsRepository> repositories)
    {
        _repositories = repositories.ToList();
    }

    public DbContext CreateContext()
    {
        return new DbContext(_repositories);
    }
}
