using Microsoft.Azure.Functions.Worker;
using Pulsar.BuildingBlocks.EventBus.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBus.Migrations.Core
{
    public class AzureFunctionInspector
    {
        public AzureFunctionInformation? GetInformation(Type azureFunction)
        {
            var func = azureFunction.GetMethods().Where(m => m.GetCustomAttribute<FunctionAttribute>() != null).First();
            var funcAttr = func.GetCustomAttribute<FunctionAttribute>();
            var serviceBusTrigger = func.GetParameters()[0].GetCustomAttribute<ServiceBusTriggerAttribute>();
            var eventName = func.GetParameters()[0].ParameterType.GetCustomAttribute<EventNameAttribute>();

            if (funcAttr is null || serviceBusTrigger is null || eventName is null)
                return null;

            var developer = Environment.GetEnvironmentVariable("ServiceBusDeveloper");

            if (developer is null)
                return null;

            if (serviceBusTrigger.TopicName is null || serviceBusTrigger.SubscriptionName is null)
                return null;

            return new AzureFunctionInformation(funcAttr.Name,
                serviceBusTrigger.TopicName.Replace("%ServiceBusDeveloper%", developer),
                serviceBusTrigger.SubscriptionName,
                eventName.Name);
        }
    }
}
