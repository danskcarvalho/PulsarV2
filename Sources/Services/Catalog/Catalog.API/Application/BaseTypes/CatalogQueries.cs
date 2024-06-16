using MongoDB.Bson;
using MongoDB.Driver;
using Pulsar.BuildingBlocks.Caching.Abstractions;
using Pulsar.BuildingBlocks.DDD.Mongo.Implementations;
using Pulsar.BuildingBlocks.DDD.Mongo.Queries;
using Pulsar.BuildingBlocks.Sync.Contracts;
using Pulsar.Services.Catalog.Domain;
using Pulsar.Services.Catalog.Domain.Aggregates.Dentes;
using Pulsar.Services.Catalog.Domain.Aggregates.Diagnosticos;
using Pulsar.Services.Catalog.Domain.Aggregates.Especialidades;
using Pulsar.Services.Catalog.Domain.Aggregates.Etnias;
using Pulsar.Services.Catalog.Domain.Aggregates.Ineps;
using Pulsar.Services.Catalog.Domain.Aggregates.Materiais;
using Pulsar.Services.Catalog.Domain.Aggregates.PrincipiosAtivos;
using Pulsar.Services.Catalog.Domain.Aggregates.Procedimentos;
using Pulsar.Services.Catalog.Domain.Aggregates.Regioes;

namespace Catalog.API.Application.BaseTypes;

public class CatalogQueries : QueryHandler
{
    public CatalogQueries(CatalogQueriesContext ctx) : base(ctx.Factory, ctx.ClusterName)
    {
        DentesCollection = GetCollection<Dente>(Constants.CollectionNames.DENTES);
        DiagnosticosCollection = GetCollection<Diagnostico>(Constants.CollectionNames.DIAGNOSTICOS);
        EspecialidadesCollection = GetCollection<Especialidade>(Constants.CollectionNames.ESPECIALIDADES);
        EtniasCollection = GetCollection<Etnia>(Constants.CollectionNames.ETNIAS);
        InepsCollection = GetCollection<Inep>(Constants.CollectionNames.INEPS);
        MateriaisCollection = GetCollection<Material>(Constants.CollectionNames.MATERIAIS);
        PrincipiosAtivosCollection = GetCollection<PrincipioAtivo>(Constants.CollectionNames.PRINCIPIOSATIVOS);
        ProcedimentosCollection = GetCollection<Procedimento>(Constants.CollectionNames.PROCEDIMENTOS);
        RegioesCollection = GetCollection<Regiao>(Constants.CollectionNames.REGIOES);
        CacheServer = ctx.CacheServer;
        Filters = new CatalogFilterContext();
    }

    protected IMongoCollection<Dente> DentesCollection { get; private set; }
    protected IMongoCollection<Diagnostico> DiagnosticosCollection { get; private set; }
    protected IMongoCollection<Especialidade> EspecialidadesCollection { get; private set; }
    protected IMongoCollection<Etnia> EtniasCollection { get; private set; }
    protected IMongoCollection<Inep> InepsCollection { get; private set; }
    protected IMongoCollection<Material> MateriaisCollection { get; private set; }
    protected IMongoCollection<PrincipioAtivo> PrincipiosAtivosCollection { get; private set; }
    protected IMongoCollection<Procedimento> ProcedimentosCollection { get; private set; }
    protected IMongoCollection<Regiao> RegioesCollection { get; private set; }
    protected CatalogFilterContext Filters { get; private set; }
    protected ICacheServer CacheServer { get; private set; }
}

public class CatalogQueriesContext
{
    public MongoDbSessionFactory Factory { get; }
    public ICacheServer CacheServer { get; }
    public string ClusterName { get; }

    public CatalogQueriesContext(MongoDbSessionFactory factory, ICacheServer cacheServer, string clusterName)
    {
        Factory = factory;
        CacheServer = cacheServer;
        ClusterName = clusterName;
    }
}

public class CatalogFilterContext
{
    public FilterDefinitionBuilder<Dente> Dentes => Builders<Dente>.Filter;
    public FilterDefinitionBuilder<Diagnostico> Diagnosticos  => Builders<Diagnostico>.Filter;
    public FilterDefinitionBuilder<Especialidade> Especialidades  => Builders<Especialidade>.Filter;
    public FilterDefinitionBuilder<Etnia> Etnias  => Builders<Etnia>.Filter;
    public FilterDefinitionBuilder<Inep> Ineps  => Builders<Inep>.Filter;
    public FilterDefinitionBuilder<Material> Materiais  => Builders<Material>.Filter;
    public FilterDefinitionBuilder<PrincipioAtivo> PrincipiosAtivos  => Builders<PrincipioAtivo>.Filter;
    public FilterDefinitionBuilder<Procedimento> Procedimentos  => Builders<Procedimento>.Filter;
    public FilterDefinitionBuilder<Regiao> Regioes  => Builders<Regiao>.Filter;

    public FilterDefinition<T> Bson<T>(BsonDocument bson) where T : class
    {
        return new BsonDocumentFilterDefinition<T>(bson);
    }
}

public static class IdentityCreateFilterExtensions
{
    public static FilterDefinition<Dente> Create(this FilterDefinitionBuilder<Dente> builder, Func<FilterDefinitionBuilder<Dente>, FilterDefinition<Dente>> lambda) => lambda(builder);
    public static FilterDefinition<Diagnostico> Create(this FilterDefinitionBuilder<Diagnostico> builder, Func<FilterDefinitionBuilder<Diagnostico>, FilterDefinition<Diagnostico>> lambda) => lambda(builder);
    public static FilterDefinition<Especialidade> Create(this FilterDefinitionBuilder<Especialidade> builder, Func<FilterDefinitionBuilder<Especialidade>, FilterDefinition<Especialidade>> lambda) => lambda(builder);
    public static FilterDefinition<Etnia> Create(this FilterDefinitionBuilder<Etnia> builder, Func<FilterDefinitionBuilder<Etnia>, FilterDefinition<Etnia>> lambda) => lambda(builder);
    public static FilterDefinition<Material> Create(this FilterDefinitionBuilder<Material> builder, Func<FilterDefinitionBuilder<Material>, FilterDefinition<Material>> lambda) => lambda(builder);
    public static FilterDefinition<PrincipioAtivo> Create(this FilterDefinitionBuilder<PrincipioAtivo> builder, Func<FilterDefinitionBuilder<PrincipioAtivo>, FilterDefinition<PrincipioAtivo>> lambda) => lambda(builder);
    public static FilterDefinition<Inep> Create(this FilterDefinitionBuilder<Inep> builder, Func<FilterDefinitionBuilder<Inep>, FilterDefinition<Inep>> lambda) => lambda(builder);
    public static FilterDefinition<Procedimento> Create(this FilterDefinitionBuilder<Procedimento> builder, Func<FilterDefinitionBuilder<Procedimento>, FilterDefinition<Procedimento>> lambda) => lambda(builder);
    public static FilterDefinition<Regiao> Create(this FilterDefinitionBuilder<Regiao> builder, Func<FilterDefinitionBuilder<Regiao>, FilterDefinition<Regiao>> lambda) => lambda(builder);


    public static FilterDefinition<Dente> Bson(this FilterDefinitionBuilder<Dente> builder, BsonDocument bson) => new BsonDocumentFilterDefinition<Dente>(bson);
    public static FilterDefinition<Diagnostico> Bson(this FilterDefinitionBuilder<Diagnostico> builder, BsonDocument bson) => new BsonDocumentFilterDefinition<Diagnostico>(bson);
    public static FilterDefinition<Especialidade> Bson(this FilterDefinitionBuilder<Especialidade> builder, BsonDocument bson) => new BsonDocumentFilterDefinition<Especialidade>(bson);
    public static FilterDefinition<Etnia> Bson(this FilterDefinitionBuilder<Etnia> builder, BsonDocument bson) => new BsonDocumentFilterDefinition<Etnia>(bson);
    public static FilterDefinition<Material> Bson(this FilterDefinitionBuilder<Material> builder, BsonDocument bson) => new BsonDocumentFilterDefinition<Material>(bson);
    public static FilterDefinition<PrincipioAtivo> Bson(this FilterDefinitionBuilder<PrincipioAtivo> builder, BsonDocument bson) => new BsonDocumentFilterDefinition<PrincipioAtivo>(bson);
    public static FilterDefinition<Inep> Bson(this FilterDefinitionBuilder<Inep> builder, BsonDocument bson) => new BsonDocumentFilterDefinition<Inep>(bson);
    public static FilterDefinition<Procedimento> Bson(this FilterDefinitionBuilder<Procedimento> builder, BsonDocument bson) => new BsonDocumentFilterDefinition<Procedimento>(bson);
    public static FilterDefinition<Regiao> Bson(this FilterDefinitionBuilder<Regiao> builder, BsonDocument bson) => new BsonDocumentFilterDefinition<Regiao>(bson);
}