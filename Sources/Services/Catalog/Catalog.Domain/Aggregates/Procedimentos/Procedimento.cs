using DDD.Contracts;
using Pulsar.Services.Catalog.Contracts.DTOs;
using Pulsar.Services.Catalog.Domain.Aggregates.Especialidades;
using Pulsar.Services.Catalog.Domain.Aggregates.Ineps;

namespace Pulsar.Services.Catalog.Domain.Aggregates.Procedimentos;

public partial class Procedimento : AggregateRoot
{
    public string Codigo { get; set; }
    public string Nome { get; set; }
    public Sexo? Sexo { get; set; }
    public Complexidade? Complexidade { get; set; }
    public int? IdadeMinimaEmDias { get; set; }
    public int? IdadeMaximaEmDias { get; set; }
    public bool? ProcedimentoAb { get; set; }
    public bool PodeInformarResultadoEspecifico { get; set; }
    public bool PodeInformarResultadoNumerico { get; set; }
    public string TermosPesquisa { get; set; }
    public List<EspecialidadeResumida> Especialidades { get; set; }
    [BsonRepresentation(BsonType.String)]
    public List<ResultadoEspecifico> ResultadosEspecificos { get; set; }
    public bool Ativo { get; set; }

    [BsonConstructor]
    public Procedimento(ObjectId id, string codigo, string nome, Sexo? sexo, Complexidade? complexidade, int? idadeMinimaEmDias, int? idadeMaximaEmDias, bool? procedimentoAb, 
        bool podeInformarResultadoEspecifico, bool podeInformarResultadoNumerico, string termosPesquisa, List<EspecialidadeResumida> especialidades, List<ResultadoEspecifico> resultadosEspecificos, bool ativo) : base(id)
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
        TermosPesquisa = termosPesquisa;
        Especialidades = especialidades;
        ResultadosEspecificos = resultadosEspecificos;
        Ativo = ativo;
    }

    public ProcedimentoDTO ToDTO()
    {
        return new ProcedimentoDTO(Id.ToString(), Codigo, Nome, Sexo, Complexidade, IdadeMinimaEmDias, IdadeMaximaEmDias, ProcedimentoAb, PodeInformarResultadoEspecifico, PodeInformarResultadoNumerico,
            Especialidades.Select(e => e.ToDTO()).ToList(), ResultadosEspecificos.ToList());
    }
}
