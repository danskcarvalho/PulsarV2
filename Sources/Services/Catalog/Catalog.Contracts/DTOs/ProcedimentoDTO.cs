using MongoDB.Bson.Serialization.Attributes;
using Pulsar.Services.Shared.Enumerations;

namespace Pulsar.Services.Catalog.Contracts.DTOs;

public class ProcedimentoDTO
{
    public string Id { get; set; }
    public string Codigo { get; set; }
    public string Nome { get; set; }
    public Sexo? Sexo { get; set; }
    public Complexidade? Complexidade { get; set; }
    public int? IdadeMinimaEmDias { get; set; }
    public int? IdadeMaximaEmDias { get; set; }
    public bool? ProcedimentoAb { get; set; }
    public bool PodeInformarResultadoEspecifico { get; set; }
    public bool PodeInformarResultadoNumerico { get; set; }
    public List<EspecialidadeDTO> Especialidades { get; set; }
    public List<ResultadoEspecifico> ResultadosEspecificos { get; set; }

    public ProcedimentoDTO(string id,
                           string codigo,
                           string nome,
                           Sexo? sexo,
                           Complexidade? complexidade,
                           int? idadeMinimaEmDias,
                           int? idadeMaximaEmDias,
                           bool? procedimentoAb,
                           bool podeInformarResultadoEspecifico,
                           bool podeInformarResultadoNumerico,
                           List<EspecialidadeDTO> especialidades,
                           List<ResultadoEspecifico> resultadosEspecificos)
    {
        Id = id;
        Codigo = codigo;
        Nome = nome;
        Sexo = sexo;
        Complexidade = complexidade;
        IdadeMinimaEmDias = idadeMinimaEmDias;
        IdadeMaximaEmDias = idadeMaximaEmDias;
        ProcedimentoAb = procedimentoAb;
        PodeInformarResultadoEspecifico = podeInformarResultadoEspecifico;
        PodeInformarResultadoNumerico = podeInformarResultadoNumerico;
        Especialidades = especialidades;
        ResultadosEspecificos = resultadosEspecificos;
    }
}
