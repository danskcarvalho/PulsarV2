using Pulsar.Services.Catalog.Domain.Aggregates.Regioes;

namespace Pulsar.Services.Catalog.Migrations.Schema;

[Migration(20240414161400, IsPersistent = true)]
public class AddIndexes : Migration
{
    public override async Task Up()
    {
        await this.UpIndexes(typeof(Regiao).Assembly);
    }
}
