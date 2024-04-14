namespace Pulsar.Services.Catalog.Domain.Aggregates.Materiais;

public class CalendarioVacinacao
{
    public int Dose { get; set; }
    public int? IdadeMinimaEmDias { get; set; }
    public int? IdadeMaximaEmDias { get; set; }
    public Sexo? Sexo { get; set; }
    public bool? Gestante { get; set; }
    public int OrdemDose { get; set; }

    [BsonConstructor]
    public CalendarioVacinacao(int dose, int? idadeMinimaEmDias, int? idadeMaximaEmDias, Sexo? sexo, bool? gestante, int ordemDose)
    {
        Dose = dose;
        IdadeMinimaEmDias = idadeMinimaEmDias;
        IdadeMaximaEmDias = idadeMaximaEmDias;
        Sexo = sexo;
        Gestante = gestante;
        OrdemDose = ordemDose;
    }
}
