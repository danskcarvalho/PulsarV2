using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using MongoDB.Bson;
using Pulsar.BuildingBlocks.Sync.Contracts;
using Pulsar.BuildingBlocks.Sync.Functions.Abstractions;
using Pulsar.BuildingBlocks.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Pulsar.BuildingBlocks.Sync.Functions.Implementations
{
    public class SyncOrchestrator<TFunctionClass> : ISyncOrchestrator<TFunctionClass> where TFunctionClass : class
    {
        private const int MAX_BATCHES_PROCESSING = 20;

        private readonly string _activityFunctionName;
        private readonly ISyncDbContextFactory _factory;

        public SyncOrchestrator(ISyncDbContextFactory factory)
        {
            foreach (var method in typeof(TFunctionClass).GetMethods())
            {
                var parameters = method.GetParameters();
                if (parameters.Length >= 1)
                {
                    var trigger = parameters[0].GetCustomAttributes<ActivityTriggerAttribute>();
                    if (trigger != null)
                    {
                        var attr = method.GetCustomAttribute<FunctionAttribute>() ?? throw new InvalidOperationException("no FunctionName on method");
                        _activityFunctionName = attr.Name;
                    }
                }
            }

            _activityFunctionName = _activityFunctionName ?? throw new InvalidOperationException("no activity function found");
            this._factory = factory;
        }
        public async Task Execute(TaskOrchestrationContext context)
        {
            var @event = context.GetInput<string>()?.FromJsonString<EntityChangedIE>() ?? throw new InvalidOperationException("no input for orchestrator");
            var data = (new PortableActivityDescription(PortableActivityDescription.PREPARE_BATCHES, @event, null)).ToJsonString();
            var strResult = await context.CallActivityAsync<string>(_activityFunctionName, data);
            var result = strResult.FromJsonString<PrepareBatchesActivityDescriptionResult>() ?? throw new InvalidOperationException("no batches to process (returned null)");

            List<Task> batchesProcessing = [];

            foreach (var batchId in result.BatchIds)
            {
                var batch = new PortableActivityDescription(PortableActivityDescription.EXECUTE_BATCH, null, batchId);
                data = batch.ToJsonString();
                batchesProcessing.Add(context.CallActivityAsync(_activityFunctionName, data));

                if (batchesProcessing.Count >= MAX_BATCHES_PROCESSING)
                {
                    await Task.WhenAll(batchesProcessing);
                    batchesProcessing.Clear();
                }
            }

            if (batchesProcessing.Count > 0)
            {
                await Task.WhenAll(batchesProcessing); 
                batchesProcessing.Clear();
            }
        }
    }
}
