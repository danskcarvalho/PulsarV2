namespace Pulsar.Services.Identity.API.Application.BaseTypes;

public class IdentityQueries : QueryHandler
{
    public IdentityQueries(MongoDbSessionFactory factory) : base(factory)
    {
        Usuarios = GetCollection<Usuario>(Constants.CollectionNames.Usuarios);
        Dominios = GetCollection<Dominio>(Constants.CollectionNames.Dominios);
        Convites = GetCollection<Convite>(Constants.CollectionNames.Convites);
        Grupos = GetCollection<Grupo>(Constants.CollectionNames.Grupos);
        Estabelecimentos = GetCollection<Estabelecimento>(Constants.CollectionNames.Estabelecimentos);
        RedesEstabelecimentos = GetCollection<RedeEstabelecimentos>(Constants.CollectionNames.RedesEstabelecimentos);
    }

    protected IMongoCollection<Usuario> Usuarios { get; private set; }
    protected IMongoCollection<Dominio> Dominios { get; private set; }
    protected IMongoCollection<Convite> Convites { get; private set; }
    protected IMongoCollection<Grupo> Grupos { get; private set; }
    protected IMongoCollection<Estabelecimento> Estabelecimentos { get; private set; }
    protected IMongoCollection<RedeEstabelecimentos> RedesEstabelecimentos { get; private set; }
}