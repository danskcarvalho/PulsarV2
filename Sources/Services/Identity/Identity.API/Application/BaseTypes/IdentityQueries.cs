using Pulsar.Services.Identity.Domain.Aggregates.Grupos;

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
        Filters = new IdentityFilterContext();
    }

    protected IMongoCollection<Usuario> UsuariosCollection { get; private set; }
    protected IMongoCollection<Dominio> DominiosCollection { get; private set; }
    protected IMongoCollection<Convite> ConvitesCollection { get; private set; }
    protected IMongoCollection<Grupo> GruposCollection { get; private set; }
    protected IMongoCollection<Estabelecimento> EstabelecimentosCollection { get; private set; }
    protected IMongoCollection<RedeEstabelecimentos> RedesEstabelecimentosCollection { get; private set; }
    protected IdentityFilterContext Filters { get; private set; }
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

public class IdentityFilterContext
{
    public FilterDefinitionBuilder<Usuario> Usuarios => Builders<Usuario>.Filter;
    public FilterDefinitionBuilder<Dominio> Dominios => Builders<Dominio>.Filter;
    public FilterDefinitionBuilder<Convite> Convites => Builders<Convite>.Filter;
    public FilterDefinitionBuilder<Grupo> Grupos => Builders<Grupo>.Filter;
    public FilterDefinitionBuilder<Estabelecimento> Estabelecimentos => Builders<Estabelecimento>.Filter;
    public FilterDefinitionBuilder<RedeEstabelecimentos> RedesEstabelecimentos => Builders<RedeEstabelecimentos>.Filter;

    public FilterDefinition<T> Bson<T>(BsonDocument bson) where T : class
    {
        return new BsonDocumentFilterDefinition<T>(bson);
    }
}

public static class IdentityCreateFilterExtensions
{
    public static FilterDefinition<Usuario> Create(this FilterDefinitionBuilder<Usuario> builder, Func<FilterDefinitionBuilder<Usuario>, FilterDefinition<Usuario>> lambda)
    {
        return lambda(builder);
    }
    public static FilterDefinition<Dominio> Create(this FilterDefinitionBuilder<Dominio> builder, Func<FilterDefinitionBuilder<Dominio>, FilterDefinition<Dominio>> lambda)
    {
        return lambda(builder);
    }
    public static FilterDefinition<Convite> Create(this FilterDefinitionBuilder<Convite> builder, Func<FilterDefinitionBuilder<Convite>, FilterDefinition<Convite>> lambda)
    {
        return lambda(builder);
    }
    public static FilterDefinition<Grupo> Create(this FilterDefinitionBuilder<Grupo> builder, Func<FilterDefinitionBuilder<Grupo>, FilterDefinition<Grupo>> lambda)
    {
        return lambda(builder);
    }
    public static FilterDefinition<Estabelecimento> Create(this FilterDefinitionBuilder<Estabelecimento> builder, Func<FilterDefinitionBuilder<Estabelecimento>, FilterDefinition<Estabelecimento>> lambda)
    {
        return lambda(builder);
    }
    public static FilterDefinition<RedeEstabelecimentos> Create(this FilterDefinitionBuilder<RedeEstabelecimentos> builder, Func<FilterDefinitionBuilder<RedeEstabelecimentos>, FilterDefinition<RedeEstabelecimentos>> lambda)
    {
        return lambda(builder);
    }

    public static FilterDefinition<Usuario> Bson(this FilterDefinitionBuilder<Usuario> builder, BsonDocument bson)
    {
        return new BsonDocumentFilterDefinition<Usuario>(bson);
    }
    public static FilterDefinition<Dominio> Bson(this FilterDefinitionBuilder<Dominio> builder, BsonDocument bson)
    {
        return new BsonDocumentFilterDefinition<Dominio>(bson);
    }
    public static FilterDefinition<Convite> Bson(this FilterDefinitionBuilder<Convite> builder, BsonDocument bson)
    {
        return new BsonDocumentFilterDefinition<Convite>(bson);
    }
    public static FilterDefinition<Grupo> Bson(this FilterDefinitionBuilder<Grupo> builder, BsonDocument bson)
    {
        return new BsonDocumentFilterDefinition<Grupo>(bson);
    }
    public static FilterDefinition<Estabelecimento> Bson(this FilterDefinitionBuilder<Estabelecimento> builder, BsonDocument bson)
    {
        return new BsonDocumentFilterDefinition<Estabelecimento>(bson);
    }
    public static FilterDefinition<RedeEstabelecimentos> Bson(this FilterDefinitionBuilder<RedeEstabelecimentos> builder, BsonDocument bson)
    {
        return new BsonDocumentFilterDefinition<RedeEstabelecimentos>(bson);
    }
}