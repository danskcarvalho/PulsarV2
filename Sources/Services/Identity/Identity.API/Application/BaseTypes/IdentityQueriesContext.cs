namespace Pulsar.Services.Identity.API.Application.BaseTypes;

public class IdentityQueriesContext
{
    public MongoDbSessionFactory Factory { get; }
    public string ClusterName { get; }

    public IdentityQueriesContext(MongoDbSessionFactory factory, string clusterName)
    {
        Factory = factory;
        ClusterName = clusterName;
    }
}