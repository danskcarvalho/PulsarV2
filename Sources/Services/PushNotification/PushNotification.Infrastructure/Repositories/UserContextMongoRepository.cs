using Pulsar.Services.PushNotification.Domain.Aggregates.UserContexts;

namespace Pulsar.Services.PushNotification.Infrastructure.Repositories;

public class UserContextMongoRepository : MongoRepository<IUserContextRepository, UserContext>, IUserContextRepository
{
	public UserContextMongoRepository(MongoDbSession? session, MongoDbSessionFactory sessionFactory) : base(session, sessionFactory)
	{
	}

	protected override string CollectionName => Constants.CollectionNames.USER_CONTEXTS;

	protected override IUserContextRepository Clone(MongoDbSession? session, MongoDbSessionFactory sessionFactory)
	{
		return new UserContextMongoRepository(session, sessionFactory);
	}
}
