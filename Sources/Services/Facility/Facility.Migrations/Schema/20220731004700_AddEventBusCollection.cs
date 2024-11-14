namespace Pulsar.Services.Identity.Migrations.Schema;

[Migration(20220731004700)]
public class AddEventBusCollection : Migration
{
    public override async Task Up()
    {
        await this.UpEventLogs();
    }
}
