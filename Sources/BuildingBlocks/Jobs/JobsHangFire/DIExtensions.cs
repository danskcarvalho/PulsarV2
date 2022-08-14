namespace Pulsar.BuildingBlocks.JobsHangFire;

public static class DIExtensions
{
    public static void AddHangfireScheduler(this IServiceCollection col)
    {
        col.AddScoped<IJobScheduler, HangFireJobScheduler>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var logger = sp.GetRequiredService<ILogger<HangFireJobScheduler>>();
            var conn = config.GetConnectionString("Hangfire");
            return new HangFireJobScheduler(conn, logger);
        });
    }
}
