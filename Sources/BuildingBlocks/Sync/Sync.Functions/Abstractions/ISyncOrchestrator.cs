using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Pulsar.BuildingBlocks.Sync.Functions.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulsar.BuildingBlocks.Sync.Functions.Abstractions
{
    public interface ISyncOrchestrator<TFunctionClass> where TFunctionClass : class
    {
        Task Execute(IDurableOrchestrationContext context);
    }
}
