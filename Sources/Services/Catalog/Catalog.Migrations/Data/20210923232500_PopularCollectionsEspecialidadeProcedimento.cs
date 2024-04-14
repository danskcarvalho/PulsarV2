using Pulsar.Services.Catalog.Domain;
using Pulsar.Services.Catalog.Domain.Aggregates.Especialidades;
using Pulsar.Services.Catalog.Domain.Aggregates.Procedimentos;

namespace Pulsar.Services.Catalog.Migrations.Data;

[Migration(20210923232500, RequiresTransaction = false)]
public class PopularCollectionsEspecialidadeProcedimento : Migration
{
    public override async Task Up()
    {
        var especialidadesCollection = GetCollection<Especialidade>(Constants.CollectionNames.ESPECIALIDADES);
        var procedimentosCollection = GetCollection<Procedimento>(Constants.CollectionNames.PROCEDIMENTOS);

        var linhas = await File.ReadAllLinesAsync(@"Data\Files\especialidades.txt");
        var especialidades = new List<Especialidade>();
        foreach (var item in linhas)
        {
            if (string.IsNullOrWhiteSpace(item))
                continue;
            especialidades.Add(item.FromBson<Especialidade>());
        }

        await especialidadesCollection.DeleteManyAsync(e => true);
        foreach (var list in especialidades.Partition(500))
        {
            await especialidadesCollection.InsertManyAsync(list);
        }

        linhas = await File.ReadAllLinesAsync(@"Data\Files\procedimentos.txt");
        var procedimentos = new List<Procedimento>();
        foreach (var item in linhas)
        {
            if (string.IsNullOrWhiteSpace(item))
                continue;
            procedimentos.Add(item.FromBson<Procedimento>());
        }

        await procedimentosCollection.DeleteManyAsync(e => true);
        foreach (var list in procedimentos.Partition(500))
        {
            await procedimentosCollection.InsertManyAsync(list);
        }
    }
}
