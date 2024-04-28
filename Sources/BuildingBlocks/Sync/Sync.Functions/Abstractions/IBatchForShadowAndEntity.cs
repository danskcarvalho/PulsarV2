using MongoDB.Bson;
using Pulsar.BuildingBlocks.DDD.Abstractions;
using Pulsar.BuildingBlocks.Sync.Contracts;

namespace Pulsar.BuildingBlocks.Sync.Functions.Abstractions;

public interface IBatchForShadowAndEntity<out TShadow, TEntity> : IBatch where TShadow : class where TEntity : class, IAggregateRoot
{
}