using Pulsar.Services.PushNotification.Domain.Aggregates.PushNotifications;

namespace Pulsar.Services.PushNotification.Infrastructure.Repositories;

public class PushNotificationMongoRepository : MongoRepository<IPushNotificationRepository, Domain.Aggregates.PushNotifications.PushNotification>, IPushNotificationRepository
{
    public PushNotificationMongoRepository(MongoDbSession? session, MongoDbSessionFactory sessionFactory) : base(session, sessionFactory)
    {
    }

    protected override string CollectionName => Constants.CollectionNames.PUSH_NOTIFICATIONS;

    protected override IPushNotificationRepository Clone(MongoDbSession? session, MongoDbSessionFactory sessionFactory)
    {
        return new PushNotificationMongoRepository(session, sessionFactory);
    }
}
