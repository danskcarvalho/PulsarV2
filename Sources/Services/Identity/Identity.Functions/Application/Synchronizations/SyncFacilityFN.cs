using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Pulsar.BuildingBlocks.Sync.Contracts;
using Pulsar.BuildingBlocks.Sync.Functions.Abstractions;

namespace Pulsar.Services.Identity.Functions.Application.Synchronizations;

public class SyncFacilityFN(
    ISyncOrchestratorStarter<SyncFacilityFN> syncOrchestratorStarter,
    ISyncOrchestrator<SyncFacilityFN> syncOrchestrator,
    ISyncActivity syncActivity)
{
    [Function("SyncFacilityStarterFN_Identity")]
    public async Task StartOrchestrator(
        [ServiceBusTrigger("%ServiceBusDeveloper%.Facility", "SyncFacilityStarterFN.Identity", Connection = "ServiceBus")] EntityChangedIE evt,
        [DurableClient] DurableTaskClient durableClient)
    {
        await syncOrchestratorStarter.Start(evt, durableClient);
    }

    [Function("SyncFacilityOrchestratorFN_Identity")]
    public async Task Orchestrator(
            [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        await syncOrchestrator.Execute(context);
    }

    [Function("SyncFacilityActivityFN_Identity")]
	public async Task<string> Activity(
			[ActivityTrigger] string input)
	{
		return await syncActivity.Execute(input);
	}
}
