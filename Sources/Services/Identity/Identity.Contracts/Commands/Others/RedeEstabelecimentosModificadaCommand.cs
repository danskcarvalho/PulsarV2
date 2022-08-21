using Pulsar.Services.Shared.DTOs;

namespace Pulsar.Services.Identity.Contracts.Commands.Others;

public class RedeEstabelecimentosEditadaOuCriadaCommand : IRequest
{
    public string RedeEstabelecimentsoId { get; set; }
    public string DominioId { get; set; }
    public string Nome { get; set; }
    public DateTime TimeStamp { get; set; }
    public AuditInfoDTO AuditInfo { get; set; }

    public RedeEstabelecimentosEditadaOuCriadaCommand(string redeEstabelecimentsoId, string dominioId, string nome, DateTime timeStamp, AuditInfoDTO auditInfo)
    {
        RedeEstabelecimentsoId = redeEstabelecimentsoId;
        DominioId = dominioId;
        Nome = nome;
        TimeStamp = timeStamp;
        AuditInfo = auditInfo;
    }
}
