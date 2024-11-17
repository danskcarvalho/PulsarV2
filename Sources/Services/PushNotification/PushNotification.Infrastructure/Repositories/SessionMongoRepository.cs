using Pulsar.Services.PushNotification.Domain.Aggregates.Sessions;

namespace Pulsar.Services.PushNotification.Infrastructure.Repositories;

public class SessionMongoRepository : MongoRepository<ISessionRepository, Session>, ISessionRepository
{
	public SessionMongoRepository(MongoDbSession? session, MongoDbSessionFactory sessionFactory) : base(session, sessionFactory)
	{
	}

	protected override string CollectionName => Constants.CollectionNames.SESSIONS;

	protected override ISessionRepository Clone(MongoDbSession? session, MongoDbSessionFactory sessionFactory)
	{
		return new SessionMongoRepository(session, sessionFactory);
	}
}
