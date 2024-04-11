using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBus.Migrations.Core;

public record AzureFunctionInformation(string FunctionName, string TopicName, string SubscriptionName, string EventName)
{
}
