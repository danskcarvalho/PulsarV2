using Pulsar.Services.PushNotification.Domain.Aggregates.PushNotifications;

namespace Pulsar.Services.PushNotification.Infrastructure.Repositories;

public class PushNotificationMongoRepository : MongoRepository<INotificacaoPushRepository, NotificacaoPush>, INotificacaoPushRepository
{
    public PushNotificationMongoRepository(MongoDbSession? session, MongoDbSessionFactory sessionFactory) : base(session, sessionFactory)
    {
    }

    protected override string CollectionName => Constants.CollectionNames.NOTIFICACOES_PUSH;

    protected override INotificacaoPushRepository Clone(MongoDbSession? session, MongoDbSessionFactory sessionFactory)
    {
        return new PushNotificationMongoRepository(session, sessionFactory);
    }
}
