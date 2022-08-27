using Pulsar.Services.Identity.Domain.Aggregates.Dominios;

namespace Pulsar.Services.Identity.Migrations.Schema;

[Migration(20220827115600)]
public class AddIndexes_2 : Migration
{
    public override async Task Up()
    {
        var dominioCol = this.Database.GetCollection<Dominio>("Dominios");
        var ix_dominio_nome_id = Builders<Dominio>.IndexKeys.Ascending(x => x.Nome).Ascending(x => x.Id);

        await dominioCol.Indexes.CreateOneAsync(new MongoDB.Driver.CreateIndexModel<Dominio>(ix_dominio_nome_id, new CreateIndexOptions()
        {
            Name = "ix_dominio_nome_id"
        }));
    }
}
