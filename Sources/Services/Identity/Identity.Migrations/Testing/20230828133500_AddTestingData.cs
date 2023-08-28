using Pulsar.Services.Identity.Domain;
using Pulsar.Services.Identity.Domain.Aggregates.Dominios;

namespace Pulsar.Services.Identity.Migrations.Testing;

[Migration(20230828133500, IsForTesting = true)]
public class AddTestingData : Migration
{
    public override async Task Up()
    {
        var dominioCollection = GetCollection<Dominio>(Constants.CollectionNames.DOMINIOS);
        await dominioCollection.InsertManyAsync(IdentityDatabase.Current.Dominios);
    }
}
