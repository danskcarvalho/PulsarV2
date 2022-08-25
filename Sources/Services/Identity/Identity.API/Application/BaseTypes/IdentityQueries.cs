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
        Cache = ctx.Cache;
    }

    protected IMongoCollection<Usuario> UsuariosCollection { get; private set; }
    protected IMongoCollection<Dominio> DominiosCollection { get; private set; }
    protected IMongoCollection<Convite> ConvitesCollection { get; private set; }
    protected IMongoCollection<Grupo> GruposCollection { get; private set; }
    protected IMongoCollection<Estabelecimento> EstabelecimentosCollection { get; private set; }
    protected IMongoCollection<RedeEstabelecimentos> RedesEstabelecimentosCollection { get; private set; }
    protected ICacheServer Cache { get; private set; }
}

public class IdentityQueriesContext
{
    public MongoDbSessionFactory Factory { get; }
    public ICacheServer Cache { get; }
    public string ClusterName { get; }

    public IdentityQueriesContext(MongoDbSessionFactory factory, ICacheServer cache, string clusterName)
    {
        Factory = factory;
        Cache = cache;
        ClusterName = clusterName;
    }
}