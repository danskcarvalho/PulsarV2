using MongoDB.Bson.Serialization.Attributes;

namespace Pulsar.Services.Catalog.Contracts.DTOs;

public class VacinaDTO : MaterialDTO
{
    public VacinaDTO(string id, string imunobiologicoNome, int imunobiologicoId, List<DoseVacinacaoDTO> doses, List<EstrategiaVacinacaoDTO> estrategias) : base(id, imunobiologicoNome, TipoMaterial.Vacina)
    {
        ImunobiologicoNome = imunobiologicoNome;
        ImunobiologicoId = imunobiologicoId;
        Doses = doses;
        Estrategias = estrategias;
    }

    public string ImunobiologicoNome { get; set; }
    public int ImunobiologicoId { get; set; }
    public List<DoseVacinacaoDTO> Doses { get; set; }
    public List<EstrategiaVacinacaoDTO> Estrategias { get; set; }


}
