﻿using Pulsar.Services.Shared.DTOs;

namespace Pulsar.Services.Identity.Contracts.Commands.Others;

public class AtualizarEstabelecimentoCmd : IRequest
{
    public string EstabelecimentoId { get; set; }
    public string DominioId { get; set; }
    public string Nome { get; set; }
    public string Cnes { get; set; }
    public List<string> Redes { get; set; }
    public bool IsAtivo { get; set; }
    public DateTime TimeStamp { get; set; }
    public AuditInfoDTO AuditInfo { get; set; }

    public AtualizarEstabelecimentoCmd(string estabelecimentoId, string dominioId, string nome, string cnes, List<string> redes, bool ativo, DateTime timeStamp, AuditInfoDTO auditInfo)
    {
        EstabelecimentoId = estabelecimentoId;
        Nome = nome;
        Cnes = cnes;
        Redes = redes;
        IsAtivo = ativo;
        TimeStamp = timeStamp;
        DominioId = dominioId;
        AuditInfo = auditInfo;
    }
}
