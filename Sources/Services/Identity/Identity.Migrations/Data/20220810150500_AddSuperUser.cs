using Pulsar.BuildingBlocks.DDD;

namespace Pulsar.Services.Identity.Migrations.Data;

[Migration(20220810150500, RequiresTransaction = true)]
public class AddSuperUser : Migration
{
    public override async Task Up()
    {
        var usuarioCollection = GetCollection<Usuario>("Usuarios");
        var sid = ObjectId.Parse("62f3f4201dbf5877ae6fe940");
        var superUser = new Usuario(
            sid,
            "Administrador",
            null,
            "administrador",
            "administrador",
            new AuditInfo(sid));
        superUser.IsAtivo = true;
        await usuarioCollection.InsertOneAsync(this.Session, superUser);
    }
}
