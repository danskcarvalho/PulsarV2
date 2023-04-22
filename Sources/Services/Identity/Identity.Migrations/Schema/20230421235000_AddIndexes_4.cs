namespace Pulsar.Services.Identity.Migrations.Schema;

[Migration(20230421235000)]
public class AddIndexes_4 : Migration
{
    public override async Task Up()
    {
        var usuarioCol = this.Database.GetCollection<Usuario>("Usuarios");
        await usuarioCol.Indexes.DropOneAsync("ix_usuarios_grupos");

        var grupoCol = this.Database.GetCollection<Grupo>("Grupos");
        var ix_grupos_usuarioIds = Builders<Grupo>.IndexKeys
            .Ascending("SubGrupos.UsuarioIds")
            .Ascending("AuditInfo.RemovidoEm");

        await grupoCol.Indexes.CreateOneAsync(new MongoDB.Driver.CreateIndexModel<Grupo>(ix_grupos_usuarioIds, new CreateIndexOptions()
        {
            Name = "ix_grupos_usuarioIds"
        }));
    }
}
