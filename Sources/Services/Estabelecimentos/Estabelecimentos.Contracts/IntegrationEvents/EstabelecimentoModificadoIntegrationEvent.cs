namespace Pulsar.Services.Estabelecimentos.Contracts.IntegrationEvents;

[EventName("Estabelecimentos:EstabelecimentoEditadoOuCriadoIntegrationEvent")]
public class EstabelecimentoModificadoIntegrationEvent : IntegrationEvent
{
    public string EstabelecimentoId { get; private set; }
    public string DominioId { get; private set; }
    public string Nome { get; private set; }
    public string Cnes { get; private set; }
    public List<string> Redes { get; private set; }
    public bool IsAtivo { get; private set; }
    public ChangeEvent Modificacao { get; private set; }

    [JsonConstructor]
    public EstabelecimentoModificadoIntegrationEvent(Guid id, DateTime creationDate, string estabelecimentoId, string dominioId, string nome, string cnes, List<string> redes, bool isAtivo, ChangeEvent modificacao) : base(id, creationDate, false)
    {
        EstabelecimentoId = estabelecimentoId;
        Nome = nome;
        Cnes = cnes;
        Redes = redes;
        IsAtivo = isAtivo;
        Modificacao = modificacao;
        DominioId = dominioId;
        Redes = redes;
    }
}
