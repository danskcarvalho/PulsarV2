namespace Pulsar.Services.Identity.API.Application.BaseTypes;

public class IdentityQueries : QueryHandler
{
    public IdentityQueries(IdentityQueriesContext ctx) : base(ctx.Factory, ctx.ClusterName)
    {
        UsuariosCollection = GetCollection<Usuario>(Constants.CollectionNames.USUARIOS);
        DominiosCollection = GetCollection<Dominio>(Constants.CollectionNames.DOMINIOS);
        ConvitesCollection = GetCollection<Convite>(Constants.CollectionNames.CONVITES);
        GruposCollection = GetCollection<Grupo>(Constants.CollectionNames.GRUPOS);
        EstabelecimentosCollection = GetCollection<Estabelecimento>(Constants.CollectionNames.ESTABELECIMENTOS);
        RedesEstabelecimentosCollection = GetCollection<RedeEstabelecimentos>(Constants.CollectionNames.REDES_ESTABELECIMENTOS);
    }

    protected IMongoCollection<Usuario> UsuariosCollection { get; private set; }
    protected IMongoCollection<Dominio> DominiosCollection { get; private set; }
    protected IMongoCollection<Convite> ConvitesCollection { get; private set; }
    protected IMongoCollection<Grupo> GruposCollection { get; private set; }
    protected IMongoCollection<Estabelecimento> EstabelecimentosCollection { get; private set; }
    protected IMongoCollection<RedeEstabelecimentos> RedesEstabelecimentosCollection { get; private set; }
}

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