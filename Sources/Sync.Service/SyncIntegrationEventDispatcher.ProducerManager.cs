using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Pulsar.BuildingBlocks.DDD.Abstractions;
using Pulsar.BuildingBlocks.Sync.Contracts;
using System.Reflection;

namespace Pulsar.BuildingBlocks.Sync.Service;

public partial class SyncIntegrationEventDispatcher
{
    class ProducerManager : IProducer
    {
        private List<IProducer> _producers = new List<IProducer>();
        private IMongoDatabase _database;
        private Assembly[] _assemblies;
        private ILogger _logger;

        public ProducerManager(IMongoDatabase database, Assembly[] assemblies, ILogger logger)
        {
            _database = database;
            _assemblies = assemblies;
            _logger = logger;
            CreateProducers();
        }

        private void CreateProducers()
        {
            foreach (var assembly in _assemblies)
            {
                var types = assembly.GetTypes().Where(t => t.GetCustomAttribute<TrackChangesAttribute>(false) != null && t.IsClass && typeof(IAggregateRoot).IsAssignableFrom(t));
                foreach (var type in types)
                {
                    _producers.Add(CreateProducer(type, _database, _logger));
                }
            }
        }

        private IProducer CreateProducer(Type type, IMongoDatabase database, ILogger logger)
        {
            return (IProducer)(Activator.CreateInstance(typeof(Producer<>).MakeGenericType(type), database, logger) ?? throw new InvalidOperationException($"can't instantiate producer for {type.FullName}"));
        }

        public async Task<ChangeEvent> Pop(CancellationToken ct)
        {
            List<Task> tasks = new List<Task>();
            object changeEventLock = new object();
            ChangeEvent? changeEvent = null;
            foreach (var item in _producers)
            {
                tasks.Add(item.TryPop(ct, pop =>
                {
                    lock (changeEventLock)
                    {
                        if (changeEvent != null)
                        {
                            return;
                        }

                        changeEvent = pop();
                    }
                }));
            }

            await Task.WhenAny(tasks);

            if (changeEvent == null)
            {
                throw new TaskCanceledException();
            }

            return changeEvent;
        }

        public ChangeEvent? TryPop()
        {
            foreach (var item in _producers)
            {
                var i = item.TryPop();

                if (i != null)
                {
                    return i;
                }
            }

            return null;
        }

        public Task Run(CancellationToken ct)
        {
            List<Task> tasks = new List<Task>();
            foreach (var item in _producers)
            {
                tasks.Add(item.Run(ct));
            }
            return Task.WhenAll(tasks);
        }

        public async Task TryPop(CancellationToken ct, Action<Func<ChangeEvent>> popFunction)
        {
            List<Task> tasks = new List<Task>();
            bool called = false;
            object calledLock = new object();
            foreach (var item in _producers)
            {
                tasks.Add(item.TryPop(ct, pop =>
                {
                    lock (calledLock)
                    {
                        if (called)
                        {
                            return;
                        }

                        called = true;
                        popFunction(pop);
                    }
                }));
            }

            await Task.WhenAny(tasks);
        }
    }
}
