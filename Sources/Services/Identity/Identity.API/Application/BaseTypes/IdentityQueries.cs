namespace Pulsar.Services.Identity.API.Application.BaseTypes;

public class IdentityQueries : QueryHandler
{
    public IdentityQueries(MongoDbSessionFactory factory, ICacheServer cache) : base(factory)
    {
        Usuarios = GetCollection<Usuario>(Constants.CollectionNames.USUARIOS);
        Dominios = GetCollection<Dominio>(Constants.CollectionNames.DOMINIOS);
        Convites = GetCollection<Convite>(Constants.CollectionNames.CONVITES);
        Grupos = GetCollection<Grupo>(Constants.CollectionNames.GRUPOS);
        Estabelecimentos = GetCollection<Estabelecimento>(Constants.CollectionNames.ESTABELECIMENTOS);
        RedesEstabelecimentos = GetCollection<RedeEstabelecimentos>(Constants.CollectionNames.REDES_ESTABELECIMENTOS);
        Cache = cache;
    }

    protected IMongoCollection<Usuario> Usuarios { get; private set; }
    protected IMongoCollection<Dominio> Dominios { get; private set; }
    protected IMongoCollection<Convite> Convites { get; private set; }
    protected IMongoCollection<Grupo> Grupos { get; private set; }
    protected IMongoCollection<Estabelecimento> Estabelecimentos { get; private set; }
    protected IMongoCollection<RedeEstabelecimentos> RedesEstabelecimentos { get; private set; }
    protected ICacheServer Cache { get; private set; }
}