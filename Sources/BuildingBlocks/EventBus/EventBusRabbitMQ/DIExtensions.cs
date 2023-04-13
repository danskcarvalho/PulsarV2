namespace Pulsar.BuildingBlocks.EventBusRabbitMQ
{
    public static class DIExtensions
    {
        public static void AddRabbitMQ(this IServiceCollection col)
        {
            col.AddSingleton<IRabbitMQPersistentConnection>(sp =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                var logger = sp.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();
                var factory = new ConnectionFactory()
                {
                    HostName = config["RabbitMQ:Connection"],
                    DispatchConsumersAsync = true
                };

                if (!string.IsNullOrEmpty(config["RabbitMQ:UserName"]))
                {
                    factory.UserName = config["RabbitMQ:UserName"]!;
                }

                if (!string.IsNullOrEmpty(config["RabbitMQ:Password"]))
                {
                    factory.Password = config["RabbitMQ:Password"]!;
                }

                var retryCount = 5;
                if (!string.IsNullOrEmpty(config["RabbitMQ:RetryCount"]))
                {
                    retryCount = int.Parse(config["RabbitMQ:RetryCount"]!);
                }

                return new DefaultRabbitMQPersistentConnection(factory, logger, retryCount);

            });
            col.AddSingleton<IEventBus, EventBusRabbitMQ>(sp =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                var subscriptionClientName = config["RabbitMQ:SubscriptionClientName"];
                var rabbitMQPersistentConnection = sp.GetRequiredService<IRabbitMQPersistentConnection>();
                var iLifetimeScope = sp.GetRequiredService<IServiceScopeFactory>();
                var logger = sp.GetRequiredService<ILogger<EventBusRabbitMQ>>();
                var eventBusSubcriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();

                var retryCount = 5;
                if (!string.IsNullOrEmpty(config["RabbitMQ:RetryCount"]))
                {
                    retryCount = int.Parse(config["RabbitMQ:RetryCount"]!);
                }

                return new EventBusRabbitMQ(rabbitMQPersistentConnection, logger, iLifetimeScope, eventBusSubcriptionsManager, subscriptionClientName, retryCount);
            });
        }
    }
}
