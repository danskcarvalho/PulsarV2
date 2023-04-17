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
            var evt = await _Producer.Pop(ct);

            if (evt.Status == IntegrationEventStatus.Pending)
                await RunPendingEvent(evt, ct);
            else if (evt.Status == IntegrationEventStatus.InProgress)
                await RunInProgressEvent(evt, ct);
        }

        private async Task RunInProgressEvent(IntegrationEventLogEntry evt, CancellationToken ct)
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
                            _Logger.LogError(e, "error while restoring event");
                            _Logger.LogInformation($"will retry in {(int)ts.TotalSeconds} seconds");
                        });

                if (DateTime.UtcNow > evt.InProgressExpirationDate)
                {
                    await retryPolicy.ExecuteAsync(async () =>
                    {
                        // -- no cancelation possible to not corrupt data
                        await _Storage.MarkEventAsFailedAsync(evt.Id, evt.Version);
                    });
                }
                else if (DateTime.UtcNow > evt.InProgressRestore)
                {
                    await retryPolicy.ExecuteAsync(async () =>
                    {
                        // -- no cancelation possible to not corrupt data
                        await _Storage.RestoreEventAsync(evt.Id, evt.Version);
                    });
                }
            }
            catch (Exception e)
            {
                _Logger.LogError(e, "error while restoring event");
            }
        }

        private async Task RunPendingEvent(IntegrationEventLogEntry evt, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return;
            try
            {
                // -- only initializes the job if its pending
                // -- no cancelation possible from this point forward to not corrupt data
                bool bInitialized = await _Storage.MarkEventAsInProgressAsync(evt.Id,
                    DateTime.UtcNow.AddHours(Constants.IN_PROGRESS_RESTORE_IN_HOURS),
                    DateTime.UtcNow.AddHours(Constants.IN_PROGRESS_TIMEOUT_IN_HOURS));

                if (bInitialized)
                {
                    var retryPolicy = Policy
                        .Handle<Exception>(e => true)
                        .WaitAndRetryAsync(3,
                            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt) * 0.5 + Math.Pow(2, retryAttempt) * Random.Shared.NextDouble()),
                            (e, ts) =>
                            {
                                _Logger.LogError(e, "error while publishing event");
                                _Logger.LogInformation($"will retry in {(int)ts.TotalSeconds} seconds");
                            });

                    var wasPublished = false;
                    try
                    {
                        // -- PublishEvent should have its own retry logic
                        await PublishEvent(evt);
                        wasPublished = true;
                    }
                    catch (Exception e)
                    {
                        _Logger.LogError(e, "error while publish event");

                        evt.Attempts.Add(new IntegrationEventLogEntrySendAttempt(DateTime.UtcNow, SerializeException(e), e.GetType().FullName));

                        var putFailedState = false;
                        if (evt.Attempts.Count < Constants.MAX_ATTEMPTS)
                        {
                            if (evt.ScheduledOn is not null)
                                evt.ScheduledOn = evt.ScheduledOn?.AddMinutes(2 * evt.Attempts.Count);
                            else
                                evt.ScheduledOn = DateTime.UtcNow.AddMinutes(2 * evt.Attempts.Count);

                            try
                            {
                                await retryPolicy.ExecuteAsync(async () =>
                                {
                                    await _Storage.RescheduleEventAsync(evt.Id, evt.Attempts, evt.ScheduledOn!.Value);
                                });
                            }
                            catch (Exception e2)
                            {
                                _Logger.LogError(e2, "error while managing event table [RescheduleEventAsync]");
                            }
                        }
                        else
                            putFailedState = true;

                        if (putFailedState)
                        {
                            try
                            {
                                await retryPolicy.ExecuteAsync(async () =>
                                {
                                    await _Storage.MarkEventAsFailedAsync(evt.Id, evt.Attempts);
                                });
                            }
                            catch (Exception e2)
                            {
                                _Logger.LogError(e2, "error while managing event table [MarkEventAsFailedAsync]");
                            }
                        }
                    }

                    if (wasPublished)
                    {
                        evt.Attempts.Add(new IntegrationEventLogEntrySendAttempt(DateTime.UtcNow));
                        try
                        {
                            await retryPolicy.ExecuteAsync(async () =>
                            {
                                await _Storage.MarkEventAsPublishedAsync(evt.Id, evt.Attempts);
                            });
                        }
                        catch (Exception e)
                        {
                            _Logger.LogError(e, "error while managing event table [MarkEventAsPublishedAsync]");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _Logger.LogError(e, "error while publishing event");
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

        private Task PublishEvent(IntegrationEventLogEntry evt)
        {
            _EventBus.Publish(evt.IntegrationEvent);
            return Task.CompletedTask;
        }
    }
}
