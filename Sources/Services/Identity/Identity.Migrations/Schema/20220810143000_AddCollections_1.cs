namespace Pulsar.Services.Identity.Migrations.Schema;

[Migration(20220810143000)]
public class AddCollections_1 : Migration
{
    public override async Task Up()
    {
        if (!(await this.Database.CollectionExists("Convites")))
            await this.Database.CreateCollectionAsync("Convites");


        if (!(await this.Database.CollectionExists("Dominios")))
            await this.Database.CreateCollectionAsync("Dominios");


        if (!(await this.Database.CollectionExists("Grupos")))
            await this.Database.CreateCollectionAsync("Grupos");


        if (!(await this.Database.CollectionExists("Estabelecimentos")))
            await this.Database.CreateCollectionAsync("Estabelecimentos");


        if (!(await this.Database.CollectionExists("RedesEstabelecimentos")))
            await this.Database.CreateCollectionAsync("RedesEstabelecimentos");


        if (!(await this.Database.CollectionExists("Usuarios")))
            await this.Database.CreateCollectionAsync("Usuarios");
    }
}
