using Pulsar.BuildingBlocks.Caching.Abstractions;
using Pulsar.BuildingBlocks.DDD.Mongo.Queries;
using Pulsar.Services.Facility.Domain.Aggregates.Estabelecimentos;

namespace Pulsar.Services.Facility.API.Application.BaseTypes;

public class FacilityQueries : QueryHandler
{
	public FacilityQueries(FacilityQueriesContext ctx) : base(ctx.Factory, ctx.ClusterName)
	{
		EstabelecimentosCollection = GetCollection<Estabelecimento>(Constants.CollectionNames.ESTABELECIMENTOS);
		CacheServer = ctx.CacheServer;
	}

	protected IMongoCollection<Estabelecimento> EstabelecimentosCollection { get; private set; }
	protected ICacheServer CacheServer { get; private set; }
}
