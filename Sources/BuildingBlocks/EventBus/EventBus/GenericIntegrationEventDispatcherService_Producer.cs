namespace Pulsar.BuildingBlocks.EventBus;

public partial class GenericIntegrationEventDispatcherService
{
    private class Producer
    {
        private Queue<IntegrationEventLogEntry> _Queue = new Queue<IntegrationEventLogEntry>();
        private HashSet<Guid> _EventsInQueue = new HashSet<Guid>();
        private IIntegrationEventLogStorage _Storage;
        private GenericIntegrationEventDispatcherServiceOptions _Options;
        private ILogger<GenericIntegrationEventDispatcherService> _Logger;

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
            await Task.WhenAll(w, p);
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
                    await Task.Delay(100);
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
                        _Logger.LogInformation($"about to sleep for {_Options.PollingTimeout} milliseconds");
                        await Task.Delay(_Options.PollingTimeout, ct);

                        if (_Queue.Count >= Constants.MAX_EVENTS_ON_QUEUE)
                            continue;

                        _Logger.LogInformation($"about to poll for up to {Constants.MAX_POLLED_EVENTS} events");
                        var events = (await _Storage.RetrieveRelevantEventLogsAsync(Constants.MAX_POLLED_EVENTS, ct)).ToList();

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
                        await _Storage.WatchRelevantEventLogsAsync((evt, ct2) =>
                        {
                            timeout = 1000;
                            if (_Queue.Count >= Constants.MAX_EVENTS_ON_QUEUE)
                                return Task.CompletedTask;

                            if (ct2.IsCancellationRequested)
                                return Task.CompletedTask;
                            if (evt.MayNeedProcessing())
                            {
                                TryProcess(evt);
                                _Logger.LogInformation($"event {evt.Id} in queue");
                            }
                            else
                                _Logger.LogInformation($"event {evt.Id} can't be processed");

                            return Task.CompletedTask;
                        }, ct);
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
        private void TryProcess(IntegrationEventLogEntry evt)
        {
            lock (_Queue)
            {
                if (_EventsInQueue.Contains(evt.Id))
                    return;

                _Queue.Enqueue(evt);
                _EventsInQueue.Add(evt.Id);
            }
        }
    }
}
