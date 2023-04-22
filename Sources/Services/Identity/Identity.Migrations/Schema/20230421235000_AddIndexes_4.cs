namespace Pulsar.Services.Identity.Migrations.Schema;

[Migration(20230421235000)]
public class AddIndexes_4 : Migration
{
    public override async Task Up()
    {
        var usuarioCol = this.Database.GetCollection<BsonDocument>("Usuarios");
        await usuarioCol.Indexes.DropOneAsync("ix_usuarios_grupos");

        var grupoCol = this.Database.GetCollection<BsonDocument>("Grupos");
        var ix_grupos_usuarioIds = Builders<BsonDocument>.IndexKeys
            .Ascending("SubGrupos.UsuarioIds")
            .Ascending("AuditInfo.RemovidoEm");

        await grupoCol.Indexes.CreateOneAsync(new MongoDB.Driver.CreateIndexModel<BsonDocument>(ix_grupos_usuarioIds, new CreateIndexOptions()
        {
            Name = "ix_grupos_usuarioIds"
        }));
    }
}
