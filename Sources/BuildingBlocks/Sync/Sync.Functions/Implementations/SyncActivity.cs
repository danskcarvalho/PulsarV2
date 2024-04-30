using Microsoft.DurableTask;
using Pulsar.BuildingBlocks.Sync.Functions.Abstractions;
using Pulsar.BuildingBlocks.Utils;

namespace Pulsar.BuildingBlocks.Sync.Functions.Implementations;

public class SyncActivity(IBatchActivity batchActivity) : ISyncActivity
{
    public async Task<string> Execute(string input)
    {
        var data = input.FromJsonString<PortableActivityDescription>() ?? throw new InvalidOperationException("invalid data for sync activity");
        if (data.Key == PortableActivityDescription.PREPARE_BATCHES)
        {
            var result = await batchActivity.Execute(new PrepareBatchesActivityDescription(data.Event!));
            return result.ToJsonString();
        }
        else if (data.Key == PortableActivityDescription.EXECUTE_NOTIFICATION)
        {
            await batchActivity.Execute(new ExecuteNotificationActivityDescription(data.Event!,
                data.TrackerAssembly!,
                data.TrackerType!,
                data.RuleName!));
            return string.Empty;
        }
        else
        {
            await batchActivity.Execute(new ExecuteBatchActivityDescription(data.BatchId!.Value));
            return string.Empty;
        }
    }
}
