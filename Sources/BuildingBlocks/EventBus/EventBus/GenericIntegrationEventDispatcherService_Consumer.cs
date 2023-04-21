namespace Pulsar.BuildingBlocks.EventBus;

public partial class GenericIntegrationEventDispatcherService
{
    private class Consumer
    {
        private readonly Producer _Producer;
        private readonly IIntegrationEventLogStorage _Storage;
        private readonly GenericIntegrationEventDispatcherServiceOptions _Options;
        private readonly ILogger<GenericIntegrationEventDispatcherService> _Logger;
        private readonly IEventBus _EventBus;

        public Consumer(
            Producer producer,
            IEventBus eventBus,
            IIntegrationEventLogStorage storage,
            GenericIntegrationEventDispatcherServiceOptions options,
            ILogger<GenericIntegrationEventDispatcherService> logger)
        {
            this._Producer = producer ?? throw new ArgumentNullException(nameof(producer));
            this._Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._Storage = storage ?? throw new ArgumentNullException(nameof(storage));
            this._Options = options ?? throw new ArgumentNullException(nameof(options));
            this._EventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus)); 
        }

        public async Task Run(CancellationToken ct)
        {
            await Task.Run(async () =>
            {
                int timeout = 1000;
                while (true)
                {
                    if (ct.IsCancellationRequested)
                    {
                        _Logger.LogInformation("consumer cancelled");
                        break;
                    }
                    try
                    {
                        await PopAndRunEvent(ct);
                        timeout = 1000;
                    }
                    catch (TaskCanceledException)
                    {
                        _Logger.LogInformation("consumer cancelled through exception");
                        if (ct.IsCancellationRequested)
                            break;
                    }
                    catch(Exception ex)
                    {
                        _Logger.LogError(ex, "error while consuming events");
                        _Logger.LogInformation($"consuming will pause for {timeout} milliseconds");
                        await Task.Delay(timeout);
                        timeout *= 2;
                        timeout = Math.Min(timeout, 5 * 60 * 1000); // --> max of 5 minutes
                    }
                }
            });
        }

        private async Task PopAndRunEvent(CancellationToken ct)
        {
            var evts = await _Producer.Pop(ct);
            try
            {
                var pending = evts.Where(e => e.Status == IntegrationEventStatus.Pending).ToList();
                var inProgress = evts.Where(e => e.Status == IntegrationEventStatus.InProgress).ToList();

                if (pending.Count != 0)
                    await RunPendingEvents(pending, ct);
                if (inProgress.Count != 0)
                    await RunInProgressEvents(inProgress, ct);
            }
            finally
            {
                _Producer.ReleaseEvents(evts);
            }
        }

        private async Task RunInProgressEvents(List<IntegrationEventLogEntry> evts, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return;
            try
            {
                var retryPolicy = Policy
                    .Handle<Exception>(e => true)
                    .WaitAndRetryAsync(3,
                        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt) * 0.5 + Math.Pow(2, retryAttempt) * Random.Shared.NextDouble()),
                        (e, ts) =>
                        {
                            _Logger.LogError(e, "error while restoring events");
                            _Logger.LogInformation($"will retry in {(int)ts.TotalSeconds} seconds");
                        });

                var inProgressExpirationDate = evts.Where(e => DateTime.UtcNow > e.InProgressExpirationDate).ToList();
                var inProgressExpirationDateIds = new HashSet<Guid>(inProgressExpirationDate.Select(e => e.Id));
                var inProgressRestore = evts.Where(e => DateTime.UtcNow > e.InProgressRestore && !inProgressExpirationDateIds.Contains(e.Id)).ToList();

                if (inProgressExpirationDate.Count != 0)
                {
                    await retryPolicy.ExecuteAsync(async () =>
                    {
                        // -- no cancelation possible to not corrupt data
                        await _Storage.MarkEventsAsFailedAsync(inProgressExpirationDate.Select(e => (e.Id, e.Version)).ToList());
                    });
                }
                if (inProgressRestore.Count != 0)
                {
                    await retryPolicy.ExecuteAsync(async () =>
                    {
                        // -- no cancelation possible to not corrupt data
                        await _Storage.RestoreEventsAsync(inProgressRestore.Select(e => (e.Id, e.Version)).ToList());
                    });
                }
            }
            catch (Exception e)
            {
                _Logger.LogError(e, "error while restoring events");
            }
        }

        private async Task RunPendingEvents(List<IntegrationEventLogEntry> evts, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return;
            try
            {
                // -- only initializes the job if its pending
                // -- no cancelation possible from this point forward to not corrupt data
                var hInitialized = await _Storage.MarkEventsAsInProgressAsync(evts.Select(e => e.Id).ToList(),
                    DateTime.UtcNow.AddHours(Constants.IN_PROGRESS_RESTORE_IN_HOURS),
                    DateTime.UtcNow.AddHours(Constants.IN_PROGRESS_TIMEOUT_IN_HOURS));

                if (hInitialized.Count != 0)
                {
                    // -- only the initialized ones
                    evts = evts.Where(e => hInitialized.Contains(e.Id)).ToList();

                    var retryPolicy = Policy
                        .Handle<Exception>(e => true)
                        .WaitAndRetryAsync(3,
                            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt) * 0.5 + Math.Pow(2, retryAttempt) * Random.Shared.NextDouble()),
                            (e, ts) =>
                            {
                                _Logger.LogError(e, "error while publishing event");
                                _Logger.LogInformation($"will retry in {(int)ts.TotalSeconds} seconds");
                            });

                    foreach (var pubResult in await PublishEvents(evts))
                    {
                        if (pubResult.Exception == null)
                        {
                            var events = evts.Where(e => pubResult.Ids.Contains(e.Id)).ToList();
                            foreach (var evt in events)
                            {
                                evt.Attempts.Add(new IntegrationEventLogEntrySendAttempt(DateTime.UtcNow));
                            }

                            try
                            {
                                await retryPolicy.ExecuteAsync(async () =>
                                {
                                    await _Storage.MarkEventsAsPublishedAsync(events.Select(e => (e.Id, e.Attempts)).ToList());
                                });
                            }
                            catch (Exception e)
                            {
                                _Logger.LogError(e, "error while managing event table [MarkEventAsPublishedAsync]");
                            }
                        }
                        else
                        {
                            _Logger.LogError(pubResult.Exception, "error while publish event");

                            var events = evts.Where(e => pubResult.Ids.Contains(e.Id)).ToList();
                            var rescheduledEvents = new List<IntegrationEventLogEntry>();
                            var failedEvents = new List<IntegrationEventLogEntry>();

                            foreach (var evt in events)
                            {
                                evt.Attempts.Add(new IntegrationEventLogEntrySendAttempt(DateTime.UtcNow, SerializeException(pubResult.Exception), pubResult.Exception.GetType().FullName));
                                if (evt.Attempts.Count < Constants.MAX_ATTEMPTS)
                                {
                                    if (evt.ScheduledOn is not null)
                                        evt.ScheduledOn = evt.ScheduledOn?.AddMinutes(2 * evt.Attempts.Count);
                                    else
                                        evt.ScheduledOn = DateTime.UtcNow.AddMinutes(2 * evt.Attempts.Count);

                                    rescheduledEvents.Add(evt);
                                }
                                else
                                {
                                    failedEvents.Add(evt);
                                }
                            }

                            if (rescheduledEvents.Count != 0)
                            {
                                try
                                {
                                    await retryPolicy.ExecuteAsync(async () =>
                                    {
                                        await _Storage.RescheduleEventsAsync(rescheduledEvents.Select(e => (e.Id, e.Attempts, e.ScheduledOn!.Value)).ToList());
                                    });
                                }
                                catch (Exception e2)
                                {
                                    _Logger.LogError(e2, "error while managing event table [RescheduleEventAsync]");
                                }
                            }

                            if (failedEvents.Count != 0)
                            {
                                try
                                {
                                    await retryPolicy.ExecuteAsync(async () =>
                                    {
                                        await _Storage.MarkEventsAsFailedAsync(failedEvents.Select(e => (e.Id, e.Attempts)).ToList());
                                    });
                                }
                                catch (Exception e2)
                                {
                                    _Logger.LogError(e2, "error while managing event table [MarkEventAsFailedAsync]");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _Logger.LogError(e, "error while publishing events");
            }
        }

        private string? SerializeException(Exception e)
        {
            try
            {
                return JsonSerializer.Serialize(e, new JsonSerializerOptions());
            }
            catch
            {
                try
                {
                    return JsonSerializer.Serialize(new { Type = e.GetType().FullName, e.Message, e.StackTrace }, new JsonSerializerOptions());
                }
                catch
                {
                    return null;
                }
            }
        }

        private async Task<List<(HashSet<Guid> Ids, Exception? Exception)>> PublishEvents(List<IntegrationEventLogEntry> evts)
        {
            return await _EventBus.Publish(evts.Select(e => (e.EventName, e.Id, e.SerializedEvent)));
        }
    }
}
