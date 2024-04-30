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
                    var trigger = parameters[0].GetCustomAttribute<ActivityTriggerAttribute>();
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
            var data = (new PortableActivityDescription(PortableActivityDescription.PREPARE_BATCHES,
                @event,
                null,
                null,
                null,
                null)).ToJsonString();
            var strResult = await context.CallActivityAsync<string>(_activityFunctionName, data);
            var result = strResult.FromJsonString<PrepareBatchesActivityDescriptionResult>() ?? 
                         throw new InvalidOperationException("no batches to process (returned null)");

            List<Task> tasksProcessing = [];

            var t1 = ExecuteBatches(context, result, tasksProcessing);
            
            var t2 = SendNotifications(context, result, @event, tasksProcessing);

            await Task.WhenAll(t1, t2);
        }

        private async Task ExecuteBatches(TaskOrchestrationContext context, PrepareBatchesActivityDescriptionResult result,
            List<Task> tasksProcessing)
        {
            string data;
            foreach (var batchId in result.BatchIds)
            {
                var batch = new PortableActivityDescription(PortableActivityDescription.EXECUTE_BATCH,
                    null,
                    batchId,
                    null,
                    null,
                    null);
                data = batch.ToJsonString();
                tasksProcessing.Add(context.CallActivityAsync(_activityFunctionName, data));

                if (tasksProcessing.Count >= MAX_BATCHES_PROCESSING)
                {
                    await Task.WhenAll(tasksProcessing);
                    tasksProcessing.Clear();
                }
            }

            if (tasksProcessing.Count > 0)
            {
                await Task.WhenAll(tasksProcessing); 
                tasksProcessing.Clear();
            }
        }

        private async Task SendNotifications(TaskOrchestrationContext context, PrepareBatchesActivityDescriptionResult result,
            EntityChangedIE @event, List<Task> tasksProcessing)
        {
            string data;
            foreach (var notification in result.SendNotifications)
            {
                var activity = new PortableActivityDescription(PortableActivityDescription.EXECUTE_NOTIFICATION,
                    @event,
                    null,
                    notification.TrackerAssembly,
                    notification.TrackerType,
                    notification.RuleName);
                data = activity.ToJsonString();
                tasksProcessing.Add(context.CallActivityAsync(_activityFunctionName, data));
                
                if (tasksProcessing.Count >= MAX_BATCHES_PROCESSING)
                {
                    await Task.WhenAll(tasksProcessing);
                    tasksProcessing.Clear();
                }
            }
            
            if (tasksProcessing.Count > 0)
            {
                await Task.WhenAll(tasksProcessing); 
                tasksProcessing.Clear();
            }
        }
    }
}
