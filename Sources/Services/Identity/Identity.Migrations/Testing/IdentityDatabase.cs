using Pulsar.BuildingBlocks.DataFactory;
using Pulsar.BuildingBlocks.DDD;
using Pulsar.Services.Identity.Domain.Aggregates.Dominios;
using Pulsar.Services.Identity.Domain.Aggregates.Usuarios;

namespace Pulsar.Services.Identity.Migrations.Testing;

public partial class IdentityDatabase
{
    public static IdentityDatabase Current { get; } = new IdentityDatabase();

    const int RNG_SEED = 62086683;
    static DateTime ReferenceDate => new DateTime(2022, 10, 10, 13, 10, 0, DateTimeKind.Utc);

    public IdentityDatabase()
    {
        Generate();
    }
}
