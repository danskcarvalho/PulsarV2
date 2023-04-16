using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Pulsar.BuildingBlocks.EventBus;

public partial class GenericIntegrationEventDispatcherService
{
    private class Producer
    {
        private Queue<IntegrationEventLogEntry> _Queue = new Queue<IntegrationEventLogEntry>();
        private List<IntegrationEventLogEntry> _BatchedEvents = new List<IntegrationEventLogEntry>();
        private HashSet<Guid> _EventsInQueue = new HashSet<Guid>();
        private IIntegrationEventLogStorage _Storage;
        private GenericIntegrationEventDispatcherServiceOptions _Options;
        private ILogger<GenericIntegrationEventDispatcherService> _Logger;
        private readonly Random _random = new Random();

        public Producer(
            ILogger<GenericIntegrationEventDispatcherService> logger,
            IIntegrationEventLogStorage storage,
            GenericIntegrationEventDispatcherServiceOptions options)
        {
            this._Options = options ?? throw new ArgumentNullException(nameof(options));
            this._Storage = storage ?? throw new ArgumentNullException(nameof(storage));
            this._Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Run(CancellationToken ct)
        {
            var w = WatchChanges(ct);
            var p = PollChanges(ct);
            var b = DispatchedBatchedEvents(ct);
            await Task.WhenAll(w, p, b);
        }
        public async Task<IntegrationEventLogEntry> Pop(CancellationToken ct)
        {
            bool delay = false;
            while (true)
            {
                if (ct.IsCancellationRequested)
                    throw new TaskCanceledException();
                if (delay)
                {
                    delay = false;
                    await Task.Delay(20);
                }
                lock (_Queue)
                {
                    if (_Queue.Count == 0)
                    {
                        delay = true;
                        continue;
                    }

                    var evt = _Queue.Dequeue();
                    _EventsInQueue.Remove(evt.Id);
                    _Logger.LogInformation($"popped event {evt.Id}");
                    return evt;
                }
            }
        }
        private async Task PollChanges(CancellationToken ct)
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                    if (ct.IsCancellationRequested)
                    {
                        _Logger.LogInformation("polling cancelled");
                        break;
                    }
                    try
                    {
                        var pollingTimeout = RandomPollingTimeout();
                        _Logger.LogInformation($"about to sleep for {pollingTimeout} milliseconds");
                        await Task.Delay(pollingTimeout, ct);

                        while (true)
                        {
                            if (ct.IsCancellationRequested)
                                break;

                            _Logger.LogInformation($"about to poll for up to {Constants.MAX_POLLED_EVENTS} events");
                            var events = (await _Storage.RetrieveRelevantEventLogsAsync(Constants.MAX_POLLED_EVENTS, ct)).ToList();
                            if (events.Count == 0)
                                break;

                            Shuffle(events);

                            _Logger.LogInformation($"polled {events.Count} events");
                            foreach (var evt in events)
                            {
                                if (ct.IsCancellationRequested)
                                    break;
                                if (evt.MayNeedProcessing())
                                {
                                    TryProcess(evt);
                                    _Logger.LogInformation($"event {evt.Id} in queue");
                                }
                                else
                                    _Logger.LogInformation($"event {evt.Id} can't be processed");
                            }

                            // -- we wait the queue to be empty
                            await QueueToBeEmpty(ct);
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        _Logger.LogInformation("polling cancelled through exception");
                        if (ct.IsCancellationRequested)
                            break;
                    }
                    catch (Exception ex)
                    {
                        _Logger.LogError(ex, "error while pooling for events");
                    }
                }
            });
        }

        private void Shuffle(List<IntegrationEventLogEntry> events)
        {
            int n = events.Count;
            while (n > 1)
            {
                n--;
                int k = _random.Next(n + 1);
                var value = events[k];
                events[k] = events[n];
                events[n] = value;
            }
        }

        private int RandomPollingTimeout()
        {
            return _random.Next(_Options.PollingTimeout);
        }

        private async Task QueueToBeEmpty(CancellationToken ct)
        {
            bool delay = false;
            while (true)
            {
                if (ct.IsCancellationRequested)
                    throw new TaskCanceledException();
                if (delay)
                {
                    delay = false;
                    await Task.Delay(20);
                }
                lock (_Queue)
                {
                    if (_Queue.Count != 0)
                    {
                        delay = true;
                        continue;
                    }

                    _Logger.LogInformation($"queue is empty");
                }
            }
        }

        private async Task WatchChanges(CancellationToken ct)
        {
            await Task.Run(async () =>
            {
                int timeout = 1000;
                while (true)
                {
                    if (ct.IsCancellationRequested)
                    {
                        _Logger.LogInformation("watching cancelled");
                        break;
                    }
                    try
                    {
                        var pollingTimeout = RandomPollingTimeout();
                        _Logger.LogInformation($"about to sleep for 1000 milliseconds");
                        await Task.Delay(pollingTimeout, ct);

                        var cancelIfQueueIsMaxedSource = new CancellationTokenSource();
                        var cancelIfQueueIsMaxedToken = cancelIfQueueIsMaxedSource.Token;
                        var compositeTokenSource = CancellationTokenSource.CreateLinkedTokenSource(ct, cancelIfQueueIsMaxedToken);
                        var compositeToken = compositeTokenSource.Token;

                        await _Storage.WatchRelevantEventLogsAsync((evt, ct2) =>
                        {
                            timeout = 1000;
                            if (_Queue.Count >= Constants.MAX_EVENTS_ON_QUEUE)
                            {
                                cancelIfQueueIsMaxedSource.Cancel();
                                return Task.CompletedTask;
                            }

                            if (ct2.IsCancellationRequested)
                                return Task.CompletedTask;
                            if (evt.MayNeedProcessing())
                            {
                                // we batch up to 1 second of watched events so we can process them in random order.
                                BatchEvent(evt);
                                _Logger.LogInformation($"event {evt.Id} batched");
                            }
                            else
                                _Logger.LogInformation($"event {evt.Id} can't be processed");

                            return Task.CompletedTask;
                        }, compositeToken);
                    }
                    catch (TaskCanceledException)
                    {
                        _Logger.LogInformation("watching cancelled through exception");
                        if (ct.IsCancellationRequested)
                            break;
                    }
                    catch (Exception ex)
                    {
                        _Logger.LogError(ex, "error while watching for events");
                        _Logger.LogInformation($"watching will pause for {timeout} milliseconds");
                        await Task.Delay(timeout);
                        timeout *= 2;
                        timeout = Math.Min(timeout, 5 * 60 * 1000); // --> max of 5 minutes
                    }
                }
            });
        }

        private async Task DispatchedBatchedEvents(CancellationToken ct)
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                    if (ct.IsCancellationRequested)
                    {
                        _Logger.LogInformation("batching cancelled");
                        break;
                    }
                    try
                    {
                        _Logger.LogInformation($"about to sleep for 1000 milliseconds");
                        await Task.Delay(1000, ct);

                        if (ct.IsCancellationRequested)
                            break;

                        var events = new List<IntegrationEventLogEntry>();
                        UnbatchEvents(events);

                        if (events.Count == 0)
                            continue;

                        Shuffle(events);

                        _Logger.LogInformation($"batched {events.Count} events");
                        foreach (var evt in events)
                        {
                            if (ct.IsCancellationRequested)
                                break;
                            if (evt.MayNeedProcessing())
                            {
                                TryProcess(evt);
                                _Logger.LogInformation($"event {evt.Id} in queue");
                            }
                            else
                                _Logger.LogInformation($"event {evt.Id} can't be processed");
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        _Logger.LogInformation("cancelled through exception");
                        if (ct.IsCancellationRequested)
                            break;
                    }
                    catch (Exception ex)
                    {
                        _Logger.LogError(ex, "error while dispatching events");
                    }
                }
            });
        }

        private void UnbatchEvents(List<IntegrationEventLogEntry> events)
        {
            lock (_BatchedEvents)
            {
                events.AddRange(_BatchedEvents);
                _BatchedEvents.Clear();
            }
        }

        private void BatchEvent(IntegrationEventLogEntry evt)
        {
            lock (_Queue)
            {
                lock (_BatchedEvents)
                {
                    if (!_EventsInQueue.Contains(evt.Id))
                        _BatchedEvents.Add(evt);
                }
            }
        }

        private bool TryProcess(IntegrationEventLogEntry evt)
        {
            lock (_Queue)
            {
                if (_EventsInQueue.Contains(evt.Id))
                    return false;

                _Queue.Enqueue(evt);
                _EventsInQueue.Add(evt.Id);
                return true;
            }
        }
    }
}
