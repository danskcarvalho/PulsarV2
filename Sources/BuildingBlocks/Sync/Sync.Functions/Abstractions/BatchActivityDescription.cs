using MongoDB.Bson;
using Pulsar.BuildingBlocks.Sync.Contracts;

namespace Pulsar.BuildingBlocks.Sync.Functions.Abstractions;

public record BatchActivityDescription;

public record BatchActivityDescription<TResult> : BatchActivityDescription;

public record PrepareBatchesActivityDescription(EntityChangedIE @Event) : BatchActivityDescription<PrepareBatchesActivityDescriptionResult>;

public record PrepareBatchesActivityDescriptionResult(List<ObjectId> BatchIds);

public record ExecuteBatchActivityDescription(ObjectId BatchId) : BatchActivityDescription;
