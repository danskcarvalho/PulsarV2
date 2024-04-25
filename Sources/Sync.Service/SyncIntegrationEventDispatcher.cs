using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Pulsar.BuildingBlocks.EventBus.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Pulsar.BuildingBlocks.Sync.Service;

public partial class SyncIntegrationEventDispatcher : ISyncIntegrationEventDispatcher
{
    private readonly ILogger<SyncIntegrationEventDispatcher> _Logger;
    private readonly IMongoDatabase _Database;
    private readonly ISaveIntegrationEventLog _EventLog;
    private Assembly[] _Assemblies;
    private SyncIntegrationEventDispatcherOptions _Options;

    public SyncIntegrationEventDispatcherOptions Options => _Options;

    public SyncIntegrationEventDispatcher(ILogger<SyncIntegrationEventDispatcher> logger, IMongoDatabase database, ISaveIntegrationEventLog eventLog, Assembly[] scanAggregateRoots,
        SyncIntegrationEventDispatcherOptions options)
    {
        _Logger = logger;
        _Database = database;
        _EventLog = eventLog;
        _Assemblies = scanAggregateRoots;
        _Options = options;
    }

    public async Task Run(CancellationToken ct)
    {
        var producer = new ProducerManager(_Database, _Assemblies, _Logger);
        var consumers = new List<Consumer>();
        for (int i = 0; i < Options.MaxConsumers; i++)
        {
            consumers.Add(new Consumer(producer, _Database, _Assemblies, _Logger, _EventLog));
        }

        List<Task> tasks = new List<Task>();
        tasks.Add(producer.Run(ct));
        foreach (var c in consumers)
        {
            tasks.Add(c.Run(ct));
        }
        await Task.WhenAll(tasks);
    }
}

public class SyncIntegrationEventDispatcherOptions
{
    public int MaxConsumers { get; set; }
}