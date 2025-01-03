﻿using Pulsar.Services.Catalog.Contracts.DTOs;
using Pulsar.Services.Catalog.Domain.Aggregates.PrincipiosAtivos;

namespace Pulsar.Services.Catalog.Domain.Aggregates.Materiais;

[BsonDiscriminator("Medicamento")]
public class Medicamento : Material
{
    public string Denominacao { get; set; }
    public string Forma { get; set; }
    public string Concentracao { get; set; }
    public string CodigoEsus { get; set; }
    public PrincipioAtivoResumido PrincipioAtivo { get; set; }
    public List<MedicamentoUnidadeFornecimento> UnidadesFornecimento { get; set; }
    [BsonRepresentation(BsonType.String)]
    public List<TipoDose> Doses { get; set; }

    [BsonConstructor]
    public Medicamento(ObjectId id, string nome, TipoMaterial tipo, string termosPesquisa, bool ativo,
        string denominacao, string forma, string concentracao, string codigoEsus, PrincipioAtivoResumido principioAtivo, List<MedicamentoUnidadeFornecimento> unidadesFornecimento, List<TipoDose> doses)
        : base(id, nome, tipo, termosPesquisa, ativo)
    {
        Denominacao = denominacao;
        Forma = forma;
        Concentracao = concentracao;
        CodigoEsus = codigoEsus;
        PrincipioAtivo = principioAtivo;
        UnidadesFornecimento = unidadesFornecimento;
        Doses = doses;
    }

    public override MaterialDTO ToDTO()
    {
        return new MedicamentoDTO(Id.ToString(), Denominacao, Forma, Concentracao, CodigoEsus, PrincipioAtivo.ToDTO(), UnidadesFornecimento.Select(u => u.ToDTO()).ToList(), Doses.ToList());
    }
}
