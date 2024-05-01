using Pulsar.BuildingBlocks.Sync.Functions.Abstractions;
using System.Reflection;
using Pulsar.BuildingBlocks.Sync.Contracts;
using Pulsar.BuildingBlocks.Utils;
using Microsoft.DurableTask.Client;
using Microsoft.Azure.Functions.Worker;

namespace Pulsar.BuildingBlocks.Sync.Functions.Implementations;

public class SyncOrchestratorStarter<TFunctionClass> : ISyncOrchestratorStarter<TFunctionClass> where TFunctionClass : class
{
    private readonly string _orchestratorFunctionName;

    public SyncOrchestratorStarter()
    {
        foreach (var method in typeof(TFunctionClass).GetMethods())
        {
            var parameters = method.GetParameters();
            if (parameters.Length >= 1)
            {
                var trigger = parameters[0].GetCustomAttribute<OrchestrationTriggerAttribute>();
                if (trigger != null)
                {
                    var attr = method.GetCustomAttribute<FunctionAttribute>() ?? throw new InvalidOperationException("no FunctionName on method");
                    _orchestratorFunctionName = attr.Name;
                }
            }
        }

        _orchestratorFunctionName = _orchestratorFunctionName ?? throw new InvalidOperationException("no activity function found");
    }

    public async Task Start(EntityChangedIE evt, DurableTaskClient client)
    {
        var data = evt.ToJsonString();
        await client.ScheduleNewOrchestrationInstanceAsync(_orchestratorFunctionName, data);
    }
}
