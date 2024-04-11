// See https://aka.ms/new-console-template for more information
using Azure.Identity;
using Azure.Messaging.ServiceBus.Administration;

// Replace with your own values
string connectionString = "<Your Service Bus namespace connection string>";
string topicName = "<Your topic name>";

// Create a ServiceBusAdministrationClient object
var ns = Environment.GetEnvironmentVariable("ServiceBus__fullyQualifiedNamespace");
ServiceBusAdministrationClient adminClient = new ServiceBusAdministrationClient(ns, new DefaultAzureCredential());

var topic = await adminClient.GetTopicAsync("danskcarvalho.identity");

var options = new CreateSubscriptionOptions("danskcarvalho.identity", "helloWorld");
options.AutoDeleteOnIdle = TimeSpan.MaxValue;
options.MaxDeliveryCount = 10;
var subs = await adminClient.CreateSubscriptionAsync(options);

var corFilter = new CorrelationRuleFilter();
corFilter.Subject = "Facilities:EstabelecimentoModificado";
await adminClient.CreateRuleAsync("danskcarvalho.identity", "helloWorld", new CreateRuleOptions("MessageLabelIs", corFilter));

// Create a topic
//await adminClient.CreateTopicAsync(topicName);

Console.WriteLine($"Topic '{topicName}' created.");

Console.WriteLine("Hello, World!");
