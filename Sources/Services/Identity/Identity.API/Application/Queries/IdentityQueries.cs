namespace Pulsar.Services.Identity.API.Application.Queries;

public class IdentityQueries : QueryHandler
{
    public IdentityQueries(MongoDbSessionFactory factory) : base(factory)
    {
        Usuarios = Database.GetCollection<Usuario>(Constants.CollectionNames.Usuarios);
        Dominios = Database.GetCollection<Dominio>(Constants.CollectionNames.Dominios);
        Convites = Database.GetCollection<Convite>(Constants.CollectionNames.Convites);
        Grupos = Database.GetCollection<Grupo>(Constants.CollectionNames.Grupos);
        Estabelecimentos = Database.GetCollection<Estabelecimento>(Constants.CollectionNames.Estabelecimentos);
        RedesEstabelecimentos = Database.GetCollection<RedeEstabelecimentos>(Constants.CollectionNames.RedesEstabelecimentos);
    }

    protected IMongoCollection<Usuario> Usuarios { get; private set; }
    protected IMongoCollection<Dominio> Dominios { get; private set; }
    protected IMongoCollection<Convite> Convites { get; private set; }
    protected IMongoCollection<Grupo> Grupos { get; private set; }
    protected IMongoCollection<Estabelecimento> Estabelecimentos { get; private set; }
    protected IMongoCollection<RedeEstabelecimentos> RedesEstabelecimentos { get; private set; }
}