namespace Pulsar.BuildingBlocks.Migrations
{
    public class MigrationRunner
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public MigrationRunner(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task Run(Assembly assembly, bool isTestingEnvironment = false)
        {
            await using (var scope = _scopeFactory.CreateAsyncScope())
            {
                var mm = scope.ServiceProvider.GetRequiredService<ScopedMigration>();
                await mm.Run(assembly, isTestingEnvironment);
            }
        }
    }
}
