using Microsoft.Extensions.Logging;
using Pulsar.BuildingBlocks.Utils;
using System.Collections.Generic;

namespace Pulsar.BuildingBlocks.EventBus;

public partial class GenericIntegrationEventDispatcherService
{
    private class Producer
    {
        private Queue<IntegrationEventLogEntry[]> _Queue = new Queue<IntegrationEventLogEntry[]>();
        private List<IntegrationEventLogEntry> _BatchedEvents = new List<IntegrationEventLogEntry>();
        private HashSet<Guid> _EventsInQueue = new HashSet<Guid>();
        private IIntegrationEventLogStorage _Storage;
        private GenericIntegrationEventDispatcherServiceOptions _Options;
        private ILogger<GenericIntegrationEventDispatcherService> _Logger;
        private readonly Random _random = new Random();
        private readonly object _producerInfoLock = new object();
        private (Guid? ProducerId, int ProducerSeq, int ProducerCount)? _producerInfo = null;

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
            var b = EnqueueBatchedEvents(ct);
            var c = CheckInProducer(ct);
            await Task.WhenAll(w, p, b, c);
        }
        public async Task<IntegrationEventLogEntry[]> Pop(CancellationToken ct)
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

                    var evts = _Queue.Dequeue();
                    _EventsInQueue.ExceptWith(evts.Select(e => e.Id));
                    _Logger.LogInformation($"popped event batch");
                    return evts;
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

                            var producerInfo = GetProducerInfo();

                            if (producerInfo == null)
                                break;

                            _Logger.LogInformation($"about to poll up to {Constants.MAX_POLLED_EVENTS} events");
                            var events = (await _Storage.RetrieveRelevantEventLogsAsync(Constants.MAX_POLLED_EVENTS, producerInfo.Value.ProducerSeq, producerInfo.Value.ProducerCount, ct)).ToList();
                            if (events.Count == 0)
                                break;

                            Shuffle(events);

                            _Logger.LogInformation($"polled {events.Count} events");
                            HashSet<Guid> enqueuedEventIds = new HashSet<Guid>();
                            foreach (var evt in events.Where(e => e.MayNeedProcessing()).Partition(Constants.EVENT_BATCH_SIZE))
                            {
                                if (ct.IsCancellationRequested)
                                    break;
                                var enq = Enqueue(evt.ToArray());
                                for (int i = 0; i < evt.Count; i++)
                                {
                                    if (enq[i])
                                    {
                                        enqueuedEventIds.Add(evt[i].Id);
                                        _Logger.LogInformation($"event {evt[i].Id} in queue");
                                    }
                                }
                            }

                            // -- we wait the queue to be empty
                            await QueueWasProcessed(enqueuedEventIds, ct);
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

        private (int ProducerSeq, int ProducerCount)? GetProducerInfo()
        {
            lock (_producerInfoLock)
            {
                if (_producerInfo == null)
                    return null;
                return (_producerInfo.Value.ProducerSeq, _producerInfo.Value.ProducerCount);
            }
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

        private async Task QueueWasProcessed(HashSet<Guid> enqueuedEventIds, CancellationToken ct)
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
                    var copy = new HashSet<Guid>(enqueuedEventIds);
                    copy.IntersectWith(_EventsInQueue);

                    if (copy.Count != 0)
                    {
                        delay = true;
                        continue;
                    }

                    _Logger.LogInformation($"queue was processed");
                    return;
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
                        _Logger.LogInformation($"about to sleep for {pollingTimeout} milliseconds");
                        await Task.Delay(pollingTimeout, ct);

                        var producerInfo = GetProducerInfo();
                        if (producerInfo == null)
                            continue;

                        var cancelIfIsMaxedSource = new CancellationTokenSource();
                        var cancelIfIsMaxedToken = cancelIfIsMaxedSource.Token;
                        var compositeTokenSource = CancellationTokenSource.CreateLinkedTokenSource(ct, cancelIfIsMaxedToken);
                        var compositeToken = compositeTokenSource.Token;

                        await _Storage.WatchRelevantEventLogsAsync((evt, ct2) =>
                        {
                            timeout = 1000;
                            lock (_BatchedEvents)
                            {
                                if (_BatchedEvents.Count >= Constants.MAX_EVENTS_ON_QUEUE)
                                {
                                    cancelIfIsMaxedSource.Cancel();
                                    return Task.CompletedTask;
                                }
                            }
                            var newProducerInfo = GetProducerInfo();
                            if (newProducerInfo == null || newProducerInfo.Value != producerInfo.Value)
                            {
                                cancelIfIsMaxedSource.Cancel();
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
                        }, producerInfo.Value.ProducerSeq, producerInfo.Value.ProducerCount, compositeToken);
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

        private async Task EnqueueBatchedEvents(CancellationToken ct)
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
                        await Task.Delay(Constants.MAX_DELAY_BATCHING, ct);

                        if (ct.IsCancellationRequested)
                            break;

                        lock (_Queue)
                        {
                            if (_Queue.Sum(x => x.Length) >= Constants.MAX_EVENTS_ON_QUEUE)
                                continue;
                        }

                        var events = new List<IntegrationEventLogEntry>();
                        UnbatchEvents(events);

                        if (events.Count == 0)
                            continue;

                        Shuffle(events);

                        _Logger.LogInformation($"batched {events.Count} events");
                        foreach (var evt in events.Where(e => e.MayNeedProcessing()).Partition(Constants.EVENT_BATCH_SIZE))
                        {
                            if (ct.IsCancellationRequested)
                                break;
                            Enqueue(evt.ToArray());
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

        private bool BatchEvent(IntegrationEventLogEntry evt)
        {
            lock (_Queue)
            {
                lock (_BatchedEvents)
                {
                    if (!_EventsInQueue.Contains(evt.Id))
                    {
                        _BatchedEvents.Add(evt);
                        return true;
                    }
                    else
                        return false;
                }
            }
        }

        private bool[] Enqueue(IntegrationEventLogEntry[] evts)
        {
            lock (_Queue)
            {
                var enqueued = new bool[evts.Length];
                List<IntegrationEventLogEntry> enqueuedEntries = new List<IntegrationEventLogEntry>();
                for (int i = 0; i < evts.Length; i++)
                {
                    var evt = evts[i];
                    if (_EventsInQueue.Contains(evt.Id))
                    {
                        enqueued[i] = false;
                        continue;
                    }

                    enqueued[i] = true;
                    enqueuedEntries.Add(evt);
                    _EventsInQueue.Add(evt.Id);
                }
                if (enqueuedEntries.Count > 0)
                    _Queue.Enqueue(enqueuedEntries.ToArray());
                return enqueued;
            }
        }

        private async Task CheckInProducer(CancellationToken ct)
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                    if (ct.IsCancellationRequested)
                    {
                        _Logger.LogInformation("check-in cancelled");
                        break;
                    }
                    try
                    {
                        var pollingTimeout = RandomPollingTimeout();
                        _Logger.LogInformation($"about to sleep for {pollingTimeout} milliseconds");
                        await Task.Delay(pollingTimeout, ct);

                        (Guid? ProducerId, int ProducerSeq, int ProducerCount)? oldProducerInfo = null;
                        lock (_producerInfoLock)
                        {
                            oldProducerInfo = _producerInfo;
                        }
                        
                        // -- no cancelation possible to not corrupt data
                        var newProducerInfo = await _Storage.CheckInProducer(_producerInfo?.ProducerId, TimeSpan.FromMilliseconds(_Options.PollingTimeout) * 3);

                        lock (_producerInfoLock)
                        {
                            _producerInfo = newProducerInfo;
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        _Logger.LogInformation("check-in cancelled through exception");
                        if (ct.IsCancellationRequested)
                            break;
                    }
                    catch (Exception ex)
                    {
                        _Logger.LogError(ex, "error while checking-in producer");
                    }
                }

                try
                {
                    (Guid? ProducerId, int ProducerSeq, int ProducerCount)? lastProducerInfo = null;
                    lock (_producerInfoLock)
                    {
                        lastProducerInfo = _producerInfo;
                    }

                    if (lastProducerInfo is not null && lastProducerInfo.Value.ProducerId != null)
                        await _Storage.CheckOutProducer(lastProducerInfo.Value.ProducerId.Value);
                }
                catch (Exception ex)
                {
                    _Logger.LogError(ex, "error while checking-out producer");
                }
            });
        }
    }
}
