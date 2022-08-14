namespace Pulsar.BuildingBlocks.JobsHangFire;

public class HangFireJobScheduler : IJobScheduler
{
    BackgroundJobClient _client;
    ILogger<HangFireJobScheduler> _logger;
    public HangFireJobScheduler(string nameOrConnectionString, ILogger<HangFireJobScheduler> logger)
    {
        _logger = logger;
        _client = new BackgroundJobClient(new SqlServerStorage(nameOrConnectionString, new SqlServerStorageOptions
        {
            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            QueuePollInterval = TimeSpan.Zero,
            UseRecommendedIsolationLevel = true,
            PrepareSchemaIfNecessary = true,
            DisableGlobalLocks = true // Migration to Schema 7 is required
        }));
    }
    public Task Enqueue<TService>(Expression<Func<TService, Task>> methodCall)
    {
        try
        {
            var jobId = _client.Enqueue(methodCall);
            _logger.LogInformation($"job created with id {jobId}");
            return Task.CompletedTask;
        }
        catch(Exception e)
        {
            _logger.LogError(e, "error creating job");
            throw;
        }
    }
}
