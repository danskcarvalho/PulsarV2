namespace Pulsar.Services.Identity.Migrations.Data;

[Migration(20220810150500, RequiresTransaction = true)]
public class AddSuperUser : Migration
{
    public override async Task Up()
    {
        var usuarioCollection = GetCollection<Usuario>("Usuarios");
        var salt = GeneralExtensions.GetSalt();
        var password = (salt + "administrador").SHA256Hash();
        var sid = ObjectId.Parse("62f3f4201dbf5877ae6fe940");
        var superUser = new Usuario(
            sid,
            "Administrador",
            null,
            "administrador",
            password,
            salt,
            new AuditInfo(sid));
        superUser.IsAtivo = true;
        await usuarioCollection.InsertOneAsync(this.Session, superUser);
    }
}
