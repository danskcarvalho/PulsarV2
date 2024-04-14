namespace Pulsar.Services.Catalog.Domain.Aggregates.Especialidades;

public class Especialidade
{
    public ObjectId Id { get; set; }
    public  string Codigo { get; set; }
    public  string Nome { get; set; }
    public  bool GeraFichaCadastroIndividual { get; set; }
    public  bool GeraFichaCadastroDomiciliarTerritorial { get; set; }
    public  bool GeraFichaAtendimentoIndividual { get; set; }
    public  bool GeraFichaProcedimentos { get; set; }
    public  bool GeraFichaAtendimentoOdontologicoIndividual { get; set; }
    public  bool GeraFichaAtividadeColetiva { get; set; }
    public  bool GeraFichaVacinacao { get; set; }
    public  bool GeraFichaVisitaDomiciliarTerritorial { get; set; }
    public  bool GeraFichaMarcadoresConsumoAlimentar { get; set; }
    public  bool GeraFichaAvaliacaoElegibilidade { get; set; }
    public  bool GeraFichaAtendimentoDomiciliar { get; set; }
    public  bool GeraFichaComplementarZikaMicrocefalia { get; set; }
    public string TermosPesquisa { get; set; }
    public bool Ativo { get; set; }

    [BsonConstructor]
    public Especialidade(ObjectId id, string codigo, string nome, bool geraFichaCadastroIndividual, bool geraFichaCadastroDomiciliarTerritorial, 
        bool geraFichaAtendimentoIndividual, bool geraFichaProcedimentos, bool geraFichaAtendimentoOdontologicoIndividual, bool geraFichaAtividadeColetiva, bool geraFichaVacinacao, 
        bool geraFichaVisitaDomiciliarTerritorial, bool geraFichaMarcadoresConsumoAlimentar, bool geraFichaAvaliacaoElegibilidade, bool geraFichaAtendimentoDomiciliar, 
        bool geraFichaComplementarZikaMicrocefalia, string termosPesquisa, bool ativo)
    {
        Id = id;
        Codigo = codigo;
        Nome = nome;
        GeraFichaCadastroIndividual = geraFichaCadastroIndividual;
        GeraFichaCadastroDomiciliarTerritorial = geraFichaCadastroDomiciliarTerritorial;
        GeraFichaAtendimentoIndividual = geraFichaAtendimentoIndividual;
        GeraFichaProcedimentos = geraFichaProcedimentos;
        GeraFichaAtendimentoOdontologicoIndividual = geraFichaAtendimentoOdontologicoIndividual;
        GeraFichaAtividadeColetiva = geraFichaAtividadeColetiva;
        GeraFichaVacinacao = geraFichaVacinacao;
        GeraFichaVisitaDomiciliarTerritorial = geraFichaVisitaDomiciliarTerritorial;
        GeraFichaMarcadoresConsumoAlimentar = geraFichaMarcadoresConsumoAlimentar;
        GeraFichaAvaliacaoElegibilidade = geraFichaAvaliacaoElegibilidade;
        GeraFichaAtendimentoDomiciliar = geraFichaAtendimentoDomiciliar;
        GeraFichaComplementarZikaMicrocefalia = geraFichaComplementarZikaMicrocefalia;
        TermosPesquisa = termosPesquisa;
        Ativo = ativo;
    }
}
