using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Pulsar.BuildingBlocks.DDD.Abstractions;
using Pulsar.BuildingBlocks.Utils.Bson;
using System.Diagnostics.Metrics;

namespace Pulsar.Services.Identity.Infrastructure.Repositories
{
    public class GrupoMongoRepository : MongoRepository<IGrupoRepository, Grupo>, IGrupoRepository
    {
        public GrupoMongoRepository(MongoDbSession? session, MongoDbSessionFactory sessionFactory) : base(session, sessionFactory)
        {
        }

        protected override string CollectionName => Constants.CollectionNames.GRUPOS;

        protected override IGrupoRepository Clone(MongoDbSession? session, MongoDbSessionFactory sessionFactory)
        {
            return new GrupoMongoRepository(session, sessionFactory);
        }
    }
}
