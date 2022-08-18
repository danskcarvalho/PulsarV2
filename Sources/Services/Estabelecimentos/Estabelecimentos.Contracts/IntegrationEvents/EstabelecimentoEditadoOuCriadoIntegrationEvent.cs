namespace Pulsar.Services.Estabelecimentos.Contracts.IntegrationEvents;

public class EstabelecimentoEditadoOuCriadoIntegrationEvent : IntegrationEvent
{
    public string EstabelecimentoId { get; private set; }
    public string DominioId { get; private set; }
    public string Nome { get; private set; }
    public string Cnes { get; private set; }
    public List<string> Redes { get; private set; }
    public bool IsAtivo { get; private set; }
    public CreatedOrEdited CriadoOuEditado { get; private set; }

    [JsonConstructor]
    public EstabelecimentoEditadoOuCriadoIntegrationEvent(Guid id, DateTime creationDate, string estabelecimentoId, string dominioId, string nome, string cnes, List<string> redes, bool isAtivo, CreatedOrEdited criadoOuEditado) : base(id, creationDate, false)
    {
        EstabelecimentoId = estabelecimentoId;
        Nome = nome;
        Cnes = cnes;
        Redes = redes;
        IsAtivo = isAtivo;
        CriadoOuEditado = criadoOuEditado;
        DominioId = dominioId;
        Redes = redes;
    }
}
