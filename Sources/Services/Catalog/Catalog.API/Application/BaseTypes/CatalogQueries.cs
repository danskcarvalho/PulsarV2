using Pulsar.BuildingBlocks.Caching.Abstractions;
using Pulsar.BuildingBlocks.DDD.Mongo.Queries;
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
    protected ICacheServer CacheServer { get; private set; }
}
