﻿using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Pulsar.BuildingBlocks.Sync.Contracts;
using Pulsar.BuildingBlocks.Sync.Functions.Abstractions;

namespace Pulsar.Services.Facility.Functions.Application.Synchronizations;

public class SyncIdentityFN(
	ISyncOrchestratorStarter<SyncIdentityFN> syncOrchestratorStarter,
	ISyncOrchestrator<SyncIdentityFN> syncOrchestrator,
	ISyncActivity syncActivity)
{
	[Function("SyncIdentityStarterFN")]
	public async Task StartOrchestrator(
		[ServiceBusTrigger("%ServiceBusDeveloper%.Identity", "SyncIdentityStarterFN.Facility", Connection = "ServiceBus")] EntityChangedIE evt,
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
	public async Task Activity(
			[ActivityTrigger] string input)
	{
		await syncActivity.Execute(input);
	}
}
