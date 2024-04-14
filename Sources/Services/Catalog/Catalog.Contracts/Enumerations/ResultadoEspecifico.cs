namespace Pulsar.Services.Catalog.Contracts.Enumerations;
public enum ResultadoEspecifico
{
    [Display(Name = "Alterada")]
    Alterada = 1,
    [Display(Name = "Alterada ≥ 90mg%")]
    AlteradaMaior90mg = 2,
    [Display(Name = "Alterado")]
    Alterado = 3,
    [Display(Name = "Falhou")]
    Falhou = 4,
    [Display(Name = "Indeterminado")]
    Indeterminado = 5,
    [Display(Name = "Não reagente")]
    NaoReagente = 6,
    [Display(Name = "Negativa")]
    Negativa = 7,
    [Display(Name = "Negativo")]
    Negativo = 8,
    [Display(Name = "Negativo para anemia")]
    NegativoParaanemia = 9,
    [Display(Name = "Normal")]
    Normal = 10,
    [Display(Name = "Outras alterações")]
    OutrasAlteracoes = 11,
    [Display(Name = "Passou")]
    Passou = 12,
    [Display(Name = "Positiva")]
    Positiva = 13,
    [Display(Name = "Positivo")]
    Positivo = 14,
    [Display(Name = "Positivo para anemia")]
    PositivoParaAnemia = 15,
    [Display(Name = "Reagente")]
    Reagente = 16,
    [Display(Name = "Sugestivo de infecção congênita")]
    SugestivoInfeccaoCongenita = 17
}

