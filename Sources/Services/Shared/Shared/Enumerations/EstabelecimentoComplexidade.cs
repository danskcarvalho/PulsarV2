namespace Pulsar.Services.Shared.Enumerations;

public enum Complexidade
{
    [Display(Name = "Não se aplica")]
    NaoSeAplica = 0,
    [Display(Name = "Atenção Básica")]
    AtencaoBasica = 1,
    [Display(Name = "Média Complexidade")]
    MediaComplexidade = 2,
    [Display(Name = "Alta Complexidade")]
    AltaComplexidade = 3,
    [Display(Name = "Média Complexidade Hospitalar")]
    MediaComplexidadeHospitalar = 4,
    [Display(Name = "Alta Complexidade Hospitalar")]
    AltaComplexidadeHospitalar = 5,
    [Display(Name = "Internação Hospitalar")]
    InternacaoHospitalar = 6
}
