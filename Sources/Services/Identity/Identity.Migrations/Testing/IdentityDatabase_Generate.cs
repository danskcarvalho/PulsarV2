using Pulsar.BuildingBlocks.DDD;
using Pulsar.Services.Identity.Domain.Aggregates.Dominios;
using Pulsar.BuildingBlocks.DataFactory;
using Pulsar.Services.Identity.Domain.Aggregates.Usuarios;

namespace Pulsar.Services.Identity.Migrations.Testing;

public partial class IdentityDatabase
{
    public List<Dominio> Dominios { get; private set; } = new List<Dominio>();

    private void Generate()
    {
        var ctx = Builder.ForSeed(RNG_SEED);
        var auditInfo = ctx.For<AuditInfo>().Recipe(g => new AuditInfo(Usuario.SuperUsuarioId, g.NextDateTime(ReferenceDate.AddDays(-100), ReferenceDate),
            null, null, null, null, null, null));
        var dominio = ctx.For<Dominio>().AutoFill(false).Recipe(g => new Dominio(g.NextObjectId(), g.NextString(), null, auditInfo())
        {
            IsAtivo = true
        });

        Dominios = dominio.Many(100)().ToList();
    }
}
