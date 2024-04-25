using MongoDB.Bson;
using Pulsar.BuildingBlocks.Sync.Contracts;

namespace Pulsar.BuildingBlocks.Sync.Service;

record ChangeEvent(string CollectionName, ObjectId Id, object? Model, List<string> ChangedProperties, DateTime When, ChangedEventKey EventKey);
