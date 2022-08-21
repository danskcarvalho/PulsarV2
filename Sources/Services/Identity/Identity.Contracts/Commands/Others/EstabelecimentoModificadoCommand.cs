namespace Pulsar.Services.Identity.Contracts.Commands.Others;

public class EstabelecimentoModificadoCommand : IRequest
{
    public string EstabelecimentoId { get; set; }
    public string DominioId { get; set; }
    public string Nome { get; set; }
    public string Cnes { get; set; }
    public List<string> Redes { get; set; }
    public bool IsAtivo { get; set; }
    public DateTime TimeStamp { get; set; }

    public EstabelecimentoModificadoCommand(string estabelecimentoId, string dominioId, string nome, string cnes, List<string> redes, bool ativo, DateTime timeStamp)
    {
        EstabelecimentoId = estabelecimentoId;
        Nome = nome;
        Cnes = cnes;
        Redes = redes;
        IsAtivo = ativo;
        TimeStamp = timeStamp;
        DominioId = dominioId;
    }
}
