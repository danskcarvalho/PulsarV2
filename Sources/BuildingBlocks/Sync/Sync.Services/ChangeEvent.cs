using MongoDB.Bson;
using Pulsar.BuildingBlocks.Sync.Contracts;

namespace Pulsar.BuildingBlocks.Sync.Services;

record ChangeEvent(string CollectionName, ObjectId Id, object? Model, List<string> ChangedProperties, DateTime When, ChangedEventKey EventKey);
