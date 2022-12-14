using Pulsar.Services.Identity.Domain.Aggregates.Dominios;
using Pulsar.Services.Identity.Domain.Aggregates.Grupos;
using Pulsar.Services.Identity.Domain.Aggregates.Others;

namespace Pulsar.Services.Identity.Migrations.Schema;

[Migration(20220810164200)]
public class AddIndexes_1 : Migration
{
    public override async Task Up()
    {
        var usuarioCol = this.Database.GetCollection<Usuario>("Usuarios");
        var ix_email = Builders<Usuario>.IndexKeys
            .Ascending(u => u.Email);
        var ix_username = Builders<Usuario>.IndexKeys
            .Ascending(u => u.NomeUsuario);
        var ix_user_text = Builders<Usuario>.IndexKeys
            .Text(x => x.TermosBusca)
            .Ascending(u => u.Email);
        var ix_dominios_bloqueados = Builders<Usuario>.IndexKeys
            .Ascending(x => x.DominiosBloqueados)
            .Ascending(x => x.Email);
        var ix_usuarios_grupos = Builders<Usuario>.IndexKeys
            .Ascending("Grupos.GrupoId")
            .Ascending("Grupos.SubGrupoId")
            .Ascending(x => x.Email);

        await usuarioCol.Indexes.CreateOneAsync(new MongoDB.Driver.CreateIndexModel<Usuario>(ix_usuarios_grupos, new CreateIndexOptions()
        {
            Name = "ix_usuarios_grupos"
        }));
        await usuarioCol.Indexes.CreateOneAsync(new MongoDB.Driver.CreateIndexModel<Usuario>(ix_dominios_bloqueados, new CreateIndexOptions()
        {
            Name = "ix_dominios_bloqueados"
        }));
        await usuarioCol.Indexes.CreateOneAsync(new MongoDB.Driver.CreateIndexModel<Usuario>(ix_email, new CreateIndexOptions()
        {
            Name = "ix_email",
            Unique = true
        }));
        await usuarioCol.Indexes.CreateOneAsync(new MongoDB.Driver.CreateIndexModel<Usuario>(ix_username, new CreateIndexOptions()
        {
            Name = "ix_username",
            Unique = true
        }));
        await usuarioCol.Indexes.CreateOneAsync(new MongoDB.Driver.CreateIndexModel<Usuario>(ix_user_text, new CreateIndexOptions()
        {
            Name = "ix_user_text"
        }));

        var estabelecimentoCol = this.Database.GetCollection<Estabelecimento>("Estabelecimentos");
        var ix_redeId = Builders<Estabelecimento>.IndexKeys.Ascending(e => e.Redes);

        await estabelecimentoCol.Indexes.CreateOneAsync(new MongoDB.Driver.CreateIndexModel<Estabelecimento>(ix_redeId, new CreateIndexOptions()
        {
            Name = "ix_redeId"
        }));

        var dominioCol = this.Database.GetCollection<Dominio>("Dominios");
        var ix_dominio_text = Builders<Dominio>.IndexKeys.Text(x => x.TermosBusca).Ascending(x => x.Nome).Ascending(x => x.Id);

        await dominioCol.Indexes.CreateOneAsync(new MongoDB.Driver.CreateIndexModel<Dominio>(ix_dominio_text, new CreateIndexOptions()
        {
            Name = "ix_dominio_text"
        }));

        var grupoCol = this.Database.GetCollection<Grupo>("Grupos");
        var ix_grupo_text = Builders<Grupo>.IndexKeys.Text(x => x.TermosBusca).Ascending(x => x.Nome).Ascending(x => x.Id);
        var ix_grupo_dominio = Builders<Grupo>.IndexKeys.Ascending(x => x.DominioId).Ascending(x => x.AuditInfo.RemovidoEm).Ascending(x => x.Nome).Ascending(x => x.Id);

        await grupoCol.Indexes.CreateOneAsync(new MongoDB.Driver.CreateIndexModel<Grupo>(ix_grupo_text, new CreateIndexOptions()
        {
            Name = "ix_grupo_text"
        }));
        await grupoCol.Indexes.CreateOneAsync(new MongoDB.Driver.CreateIndexModel<Grupo>(ix_grupo_dominio, new CreateIndexOptions()
        {
            Name = "ix_grupo_dominio"
        }));
    }
}
