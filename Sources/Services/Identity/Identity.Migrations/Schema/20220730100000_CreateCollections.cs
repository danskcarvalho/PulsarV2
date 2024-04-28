using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pulsar.Services.Facility.Contracts.Shadows;

namespace Pulsar.Services.Identity.Migrations.Schema
{
    [Migration(20220730100000)]
    public class CreateCollections : Migration
    {
        public override async Task Up()
        {
            var collation = new Collation("pt", caseLevel: false);
            
            await CreateCollection("Convites", collation);
            await CreateCollection("Dominios", collation);
            await CreateCollection("Grupos", collation);
            await CreateCollection("Usuarios", collation);
            
            // shadows
            await CreateShadowCollection<EstabelecimentoShadow>(collation);
            await CreateShadowCollection<RedeEstabelecimentosShadow>(collation);
        }
    }
}
