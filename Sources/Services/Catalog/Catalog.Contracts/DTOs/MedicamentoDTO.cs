namespace Pulsar.Services.Catalog.Contracts.DTOs;

public class MedicamentoDTO : MaterialDTO
{
    public MedicamentoDTO(string id, string denominacao, string forma, string concentracao, string codigoEsus, PrincipioAtivoDTO principioAtivo, List<MedicamentoUnidadeFornecimentoDTO> unidadesFornecimento, List<TipoDose> doses)
         : base(id, denominacao, TipoMaterial.Medicamento)
    {
        Denominacao = denominacao;
        Forma = forma;
        Concentracao = concentracao;
        CodigoEsus = codigoEsus;
        PrincipioAtivo = principioAtivo;
        UnidadesFornecimento = unidadesFornecimento;
        Doses = doses;
    }

    public string Denominacao { get; set; }
    public string Forma { get; set; }
    public string Concentracao { get; set; }
    public string CodigoEsus { get; set; }
    public PrincipioAtivoDTO PrincipioAtivo { get; set; }
    public List<MedicamentoUnidadeFornecimentoDTO> UnidadesFornecimento { get; set; }
    public List<TipoDose> Doses { get; set; }
}
