namespace Pulsar.Services.Identity.Migrations.Schema;

[Migration(20220810164200)]
public class AddIndexes_1 : Migration
{
    public override async Task Up()
    {
        var usuarioCol = this.Database.GetCollection<BsonDocument>("Usuarios");
        var ix_email = Builders<BsonDocument>.IndexKeys
            .Ascending("Email");
        var ix_username = Builders<BsonDocument>.IndexKeys
            .Ascending("NomeUsuario");
        var ix_user_text = Builders<BsonDocument>.IndexKeys
            .Text("TermosBusca")
            .Ascending("Email");
        var ix_dominios_bloqueados = Builders<BsonDocument>.IndexKeys
            .Ascending("DominiosBloqueados")
            .Ascending("Email");
        var ix_usuarios_grupos = Builders<BsonDocument>.IndexKeys
            .Ascending("Grupos.GrupoId")
            .Ascending("Grupos.SubGrupoId")
            .Ascending("Email");

        await usuarioCol.Indexes.CreateOneAsync(new MongoDB.Driver.CreateIndexModel<BsonDocument>(ix_usuarios_grupos, new CreateIndexOptions()
        {
            Name = "ix_usuarios_grupos"
        }));
        await usuarioCol.Indexes.CreateOneAsync(new MongoDB.Driver.CreateIndexModel<BsonDocument>(ix_dominios_bloqueados, new CreateIndexOptions()
        {
            Name = "ix_dominios_bloqueados"
        }));
        await usuarioCol.Indexes.CreateOneAsync(new MongoDB.Driver.CreateIndexModel<BsonDocument>(ix_email, new CreateIndexOptions()
        {
            Name = "ix_email",
            Unique = true
        }));
        await usuarioCol.Indexes.CreateOneAsync(new MongoDB.Driver.CreateIndexModel<BsonDocument>(ix_username, new CreateIndexOptions()
        {
            Name = "ix_username",
            Unique = true
        }));
        await usuarioCol.Indexes.CreateOneAsync(new MongoDB.Driver.CreateIndexModel<BsonDocument>(ix_user_text, new CreateIndexOptions()
        {
            Name = "ix_user_text"
        }));

        var estabelecimentoCol = this.Database.GetCollection<BsonDocument>("Estabelecimentos");
        var ix_redeId = Builders<BsonDocument>.IndexKeys.Ascending("Redes");

        await estabelecimentoCol.Indexes.CreateOneAsync(new MongoDB.Driver.CreateIndexModel<BsonDocument>(ix_redeId, new CreateIndexOptions()
        {
            Name = "ix_redeId"
        }));

        var dominioCol = this.Database.GetCollection<BsonDocument>("Dominios");
        var ix_dominio_text = Builders<BsonDocument>.IndexKeys.Text("TermosBusca").Ascending("Nome").Ascending("Id");

        await dominioCol.Indexes.CreateOneAsync(new MongoDB.Driver.CreateIndexModel<BsonDocument>(ix_dominio_text, new CreateIndexOptions()
        {
            Name = "ix_dominio_text"
        }));

        var grupoCol = this.Database.GetCollection<BsonDocument>("Grupos");
        var ix_grupo_text = Builders<BsonDocument>.IndexKeys.Text("TermosBusca").Ascending("Nome").Ascending("Id");
        var ix_grupo_dominio = Builders<BsonDocument>.IndexKeys.Ascending("DominioId").Ascending("AuditInfo.RemovidoEm").Ascending("Nome").Ascending("Id");

        await grupoCol.Indexes.CreateOneAsync(new MongoDB.Driver.CreateIndexModel<BsonDocument>(ix_grupo_text, new CreateIndexOptions()
        {
            Name = "ix_grupo_text"
        }));
        await grupoCol.Indexes.CreateOneAsync(new MongoDB.Driver.CreateIndexModel<BsonDocument>(ix_grupo_dominio, new CreateIndexOptions()
        {
            Name = "ix_grupo_dominio"
        }));
    }
}
