using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Pulsar.BuildingBlocks.Sync.Contracts;
using Pulsar.BuildingBlocks.Sync.Functions.Abstractions;

namespace Pulsar.Services.PushNotification.Functions.Application.Synchronizations;

public class SyncIdentityFN(
	ISyncOrchestratorStarter<SyncIdentityFN> syncOrchestratorStarter,
	ISyncOrchestrator<SyncIdentityFN> syncOrchestrator,
	ISyncActivity syncActivity)
{
	[Function("SyncIdentityStarterFN")]
	public async Task StartOrchestrator(
		[ServiceBusTrigger("%ServiceBusDeveloper%.Identity", "SyncIdentityStarterFN.PushNotification", Connection = "ServiceBus")] EntityChangedIE evt,
		[DurableClient] DurableTaskClient durableClient)
	{
		await syncOrchestratorStarter.Start(evt, durableClient);
	}

	[Function("SyncIdentityOrchestratorFN")]
	public async Task Orchestrator(
			[OrchestrationTrigger] TaskOrchestrationContext context)
	{
		await syncOrchestrator.Execute(context);
	}

	[Function("SyncIdentityActivityFN")]
	public async Task<string> Activity(
			[ActivityTrigger] string input)
	{
		return await syncActivity.Execute(input);
	}
}
