using Pulsar.Services.Catalog.Domain;
using Pulsar.Services.Catalog.Domain.Aggregates.Dentes;
using Pulsar.Services.Catalog.Domain.Aggregates.Diagnosticos;
using Pulsar.Services.Catalog.Domain.Aggregates.Etnias;
using Pulsar.Services.Catalog.Domain.Aggregates.Ineps;
using Pulsar.Services.Catalog.Domain.Aggregates.Materiais;
using Pulsar.Services.Catalog.Domain.Aggregates.PrincipiosAtivos;
using Pulsar.Services.Catalog.Domain.Aggregates.Regioes;

namespace Pulsar.Services.Catalog.Migrations.Data;

[Migration(20210927200800, RequiresTransaction = false)]
public class PopularCollectionsFixas : Migration
{
    public override async Task Up()
    {
        var dentesCollection = GetCollection<Dente>(Constants.CollectionNames.DENTES);
        var diagnosticosCollection = GetCollection<Diagnostico>(Constants.CollectionNames.DIAGNOSTICOS);
        var etniasCollection = GetCollection<Etnia>(Constants.CollectionNames.ETNIAS);
        var inepsCollection = GetCollection<Inep>(Constants.CollectionNames.INEPS);
        var materiaisCollection = GetCollection<Material>(Constants.CollectionNames.MATERIAIS);
        var principiosAtivosCollection = GetCollection<PrincipioAtivo>(Constants.CollectionNames.PRINCIPIOSATIVOS);
        var regioesCollection = GetCollection<Regiao>(Constants.CollectionNames.REGIOES);

        //dentes
        var linhas = await File.ReadAllLinesAsync(@"Data\Files\dentes.txt");
        var dentes = new List<Dente>();
        foreach (var item in linhas)
        {
            if (string.IsNullOrWhiteSpace(item))
                continue;
            dentes.Add(item.FromBson<Dente>());
        }

        await dentesCollection.DeleteManyAsync(e => true);
        foreach (var list in dentes.Partition(500))
        {
            await dentesCollection.InsertManyAsync(list);
        }

        //diagnósticos
        linhas = await File.ReadAllLinesAsync(@"Data\Files\diagnosticos.txt");
        var diagnosticos = new List<Diagnostico>();
        foreach (var item in linhas)
        {
            if (string.IsNullOrWhiteSpace(item))
                continue;
            diagnosticos.Add(item.FromBson<Diagnostico>());
        }

        await diagnosticosCollection.DeleteManyAsync(e => true);
        foreach (var list in diagnosticos.Partition(500))
        {
            await diagnosticosCollection.InsertManyAsync(list);
        }

        //etnias
        linhas = await File.ReadAllLinesAsync(@"Data\Files\etnias.txt");
        var etnias = new List<Etnia>();
        foreach (var item in linhas)
        {
            if (string.IsNullOrWhiteSpace(item))
                continue;
            etnias.Add(item.FromBson<Etnia>());
        }

        await etniasCollection.DeleteManyAsync(e => true);
        foreach (var list in etnias.Partition(500))
        {
            await etniasCollection.InsertManyAsync(list);
        }

        //ineps
        linhas = await File.ReadAllLinesAsync(@"Data\Files\ineps.txt");
        var ineps = new List<Inep>();
        foreach (var item in linhas)
        {
            if (string.IsNullOrWhiteSpace(item))
                continue;
            ineps.Add(item.FromBson<Inep>());
        }

        await inepsCollection.DeleteManyAsync(e => true);
        foreach (var list in ineps.Partition(500))
        {
            await inepsCollection.InsertManyAsync(list);
        }

        //materiais
        linhas = await File.ReadAllLinesAsync(@"Data\Files\materiais.txt");
        var materiais = new List<Material>();
        foreach (var item in linhas)
        {
            if (string.IsNullOrWhiteSpace(item))
                continue;
            materiais.Add(item.FromBson<Material>());
        }

        await materiaisCollection.DeleteManyAsync(e => true);
        foreach (var list in materiais.Partition(500))
        {
            await materiaisCollection.InsertManyAsync(list);
        }

        //principios ativos
        linhas = await File.ReadAllLinesAsync(@"Data\Files\principios_ativos.txt");
        var principiosAtivos = new List<PrincipioAtivo>();
        foreach (var item in linhas)
        {
            if (string.IsNullOrWhiteSpace(item))
                continue;
            principiosAtivos.Add(item.FromBson<PrincipioAtivo>());
        }

        await principiosAtivosCollection.DeleteManyAsync(e => true);
        foreach (var list in principiosAtivos.Partition(500))
        {
            await principiosAtivosCollection.InsertManyAsync(list);
        }

        //regiões
        linhas = await File.ReadAllLinesAsync(@"Data\Files\regioes.txt");
        var regioes = new List<Regiao>();
        foreach (var item in linhas)
        {
            if (string.IsNullOrWhiteSpace(item))
                continue;
            regioes.Add(item.FromBson<Regiao>());
        }

        await regioesCollection.DeleteManyAsync(e => true);
        foreach (var list in regioes.Partition(500))
        {
            await regioesCollection.InsertManyAsync(list);
        }
    }
}

