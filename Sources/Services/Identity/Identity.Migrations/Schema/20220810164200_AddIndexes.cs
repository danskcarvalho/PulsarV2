using Pulsar.Services.Facility.Contracts.Shadows;
using Pulsar.Services.Identity.Domain.Aggregates.Usuarios;

namespace Pulsar.Services.Identity.Migrations.Schema;

[Migration(20220810164200, IsPersistent = true)]
public class AddIndexes : Migration
{
    public override async Task Up()
    {
        await this.UpIndexes(
            typeof(Usuario).Assembly,
            // shadows
            typeof(EstabelecimentoShadow).Assembly
            );
    }
}
