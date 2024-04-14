namespace Pulsar.Services.Catalog.Domain.Aggregates.Materiais;

[BsonDiscriminator("Vacina")]
public class Vacina : Material
{
    public string ImunobiologicoNome { get; set; }
    public int ImunobiologicoId { get; set; }
    public int? OrdemCalendario { get; set; }
    public List<DoseVacinacao> Doses { get; set; }
    public List<EstrategiaVacinacao> Estrategias { get; set; }
    public List<CalendarioVacinacao> Calendario { get; set; }

    [BsonConstructor]
    public Vacina(ObjectId id, string nome, TipoMaterial tipo, string termosPesquisa, bool ativo, 
        string imunobiologicoNome, int imunobiologicoId, int? ordemCalendario, List<DoseVacinacao> doses, List<EstrategiaVacinacao> estrategias, List<CalendarioVacinacao> calendario)
        : base(id, nome, tipo, termosPesquisa, ativo)
    {
        ImunobiologicoNome = imunobiologicoNome;
        ImunobiologicoId = imunobiologicoId;
        OrdemCalendario = ordemCalendario;
        Doses = doses;
        Estrategias = estrategias;
        Calendario = calendario;
    }
}
