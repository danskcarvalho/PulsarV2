using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulsar.Services.Identity.Migrations.Schema
{
    [Migration(20220730100000)]
    public class CreateCollections : Migration
    {
        public override async Task Up()
        {
            await CreateCollection("Convites");
            await CreateCollection("Dominios");
            await CreateCollection("Grupos");
            await CreateCollection("Estabelecimentos");
            await CreateCollection("RedesEstabelecimentos");
            await CreateCollection("Usuarios");
        }

        private async Task CreateCollection(string collectionName)
        {
            await this.Database.CreateCollectionAsync(collectionName, new CreateCollectionOptions()
            {
                Collation = new Collation("pt", caseLevel: false)
            });
        }
    }
}
