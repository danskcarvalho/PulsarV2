namespace Pulsar.Services.Identity.Migrations.Schema;

[Migration(20220827115600)]
public class AddIndexes_2 : Migration
{
    public override async Task Up()
    {
        var dominioCol = this.Database.GetCollection<BsonDocument>("Dominios");
        var ix_dominio_nome_id = Builders<BsonDocument>.IndexKeys.Ascending("Nome").Ascending("Id");

        await dominioCol.Indexes.CreateOneAsync(new MongoDB.Driver.CreateIndexModel<BsonDocument>(ix_dominio_nome_id, new CreateIndexOptions()
        {
            Name = "ix_dominio_nome_id"
        }));
    }
}
