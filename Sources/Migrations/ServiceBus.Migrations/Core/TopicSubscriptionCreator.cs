using Azure.Identity;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBus.Migrations.Core;

public class TopicSubscriptionCreator
{
    private const long DEFAULT_TOPIC_SIZE_IN_MEGABYTES = 1024;
    public IConfiguration Configuration { get; }

    public TopicSubscriptionCreator(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public async Task Create(AzureFunctionInformation functionInformation)
    {
        var topicSizeInMegabytes = Configuration.GetValue<long?>("ServiceBus:TopicSizeInMegabytes") ?? DEFAULT_TOPIC_SIZE_IN_MEGABYTES;
        var enablePartitioning = Configuration.GetValue<bool>("ServiceBus:EnablePartitioning");

        var ns = Environment.GetEnvironmentVariable("ServiceBus__fullyQualifiedNamespace");
        var adminClient = new ServiceBusAdministrationClient(ns, new DefaultAzureCredential());

        if (!await adminClient.TopicExistsAsync(functionInformation.TopicName))
        {
            await adminClient.CreateTopicAsync(new CreateTopicOptions(functionInformation.TopicName)
            {
                MaxSizeInMegabytes = topicSizeInMegabytes,
                DefaultMessageTimeToLive = TimeSpan.FromDays(14),
                EnablePartitioning = enablePartitioning,
                AutoDeleteOnIdle = TimeSpan.MaxValue,
                RequiresDuplicateDetection = true,
                DuplicateDetectionHistoryTimeWindow = TimeSpan.FromMinutes(30),
                SupportOrdering = false
            });
        }

        if (!await adminClient.SubscriptionExistsAsync(functionInformation.TopicName, functionInformation.SubscriptionName))
        {
            var options = new CreateSubscriptionOptions(functionInformation.TopicName, functionInformation.SubscriptionName);
            options.AutoDeleteOnIdle = TimeSpan.MaxValue;
            options.MaxDeliveryCount = 15;
            options.DeadLetteringOnMessageExpiration = true;
            options.DefaultMessageTimeToLive = TimeSpan.FromDays(14);
            await adminClient.CreateSubscriptionAsync(options);
        }

        if (await adminClient.RuleExistsAsync(functionInformation.TopicName, functionInformation.SubscriptionName, "SubjectIs"))
        {
            await adminClient.DeleteRuleAsync(functionInformation.TopicName, functionInformation.SubscriptionName, "SubjectIs");
        }

        var filter = new CorrelationRuleFilter();
        filter.Subject = functionInformation.EventName;
        await adminClient.CreateRuleAsync(functionInformation.TopicName, functionInformation.SubscriptionName,
            new CreateRuleOptions("SubjectIs", filter));
    }

    public async Task EnsureTopic(string topicName)
    {
        var developer = Environment.GetEnvironmentVariable("ServiceBusDeveloper");
        topicName = topicName.Replace("%ServiceBusDeveloper%", developer);
        var topicSizeInMegabytes = Configuration.GetValue<long?>("ServiceBus:TopicSizeInMegabytes") ?? DEFAULT_TOPIC_SIZE_IN_MEGABYTES;
        var enablePartitioning = Configuration.GetValue<bool>("ServiceBus:EnablePartitioning");

        var ns = Environment.GetEnvironmentVariable("ServiceBus__fullyQualifiedNamespace");
        var adminClient = new ServiceBusAdministrationClient(ns, new DefaultAzureCredential());

        if (!await adminClient.TopicExistsAsync(topicName))
        {
            await adminClient.CreateTopicAsync(new CreateTopicOptions(topicName)
            {
                MaxSizeInMegabytes = topicSizeInMegabytes,
                DefaultMessageTimeToLive = TimeSpan.FromDays(14),
                EnablePartitioning = enablePartitioning,
                AutoDeleteOnIdle = TimeSpan.MaxValue,
                RequiresDuplicateDetection = true,
                DuplicateDetectionHistoryTimeWindow = TimeSpan.FromMinutes(30),
                SupportOrdering = false
            });
        }
    }
}
