namespace Pulsar.Services.Estabelecimentos.Contracts.IntegrationEvents;

public class RedeEstabelecimentosEditadoOuCriadoIntegrationEvent : IntegrationEvent
{
    public string RedeEstabelecimentosId { get; private set; }
    public string DominioId { get; private set; }
    public string Nome { get; private set; }

    [JsonConstructor]
    public RedeEstabelecimentosEditadoOuCriadoIntegrationEvent(Guid id, DateTime creationDate, string redeEstabelecimentosId, string dominioId, string nome) : base(id, creationDate, false)
    {
        RedeEstabelecimentosId = redeEstabelecimentosId;
        Nome = nome;
        DominioId = dominioId;
    }
}
