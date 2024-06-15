namespace Pulsar.Services.Catalog.Domain.Aggregates.Diagnosticos;

public class Diagnostico : AggregateRoot
{
    public TipoDiagnostico Tipo { get; set; }
    public string Codigo { get; set; }
    public string Nome { get; set; }
    public Sexo? Sexo { get; set; }
    public string TermosPesquisa { get; set; }
    public bool Ativo { get; set; }
    public List<DiagnosticoResumido> Correlatos { get; set; }

    [BsonConstructor]
    public Diagnostico(ObjectId id, TipoDiagnostico tipo, string codigo, string nome, Sexo? sexo, string termosPesquisa, bool ativo, List<DiagnosticoResumido> correlatos) : base(id)
    {
        Id = id;
        Tipo = tipo;
        Codigo = codigo;
        Nome = nome;
        Sexo = sexo;
        TermosPesquisa = termosPesquisa;
        Ativo = ativo;
        Correlatos = correlatos;
    }
}
