namespace Pulsar.Services.Identity.Contracts.Commands.Others;

public class RedeEstabelecimentosEditadoOuCriadoCommand : IRequest
{
    public string RedeEstabelecimentsoId { get; set; }
    public string DominioId { get; set; }
    public string Nome { get; set; }
    public DateTime TimeStamp { get; set; }

    public RedeEstabelecimentosEditadoOuCriadoCommand(string redeEstabelecimentsoId, string dominioId, string nome, DateTime timeStamp)
    {
        RedeEstabelecimentsoId = redeEstabelecimentsoId;
        DominioId = dominioId;
        Nome = nome;
        TimeStamp = timeStamp;
    }
}
