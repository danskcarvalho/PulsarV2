namespace Pulsar.Services.Catalog.Contracts.Enumerations;
public enum TipoMedicamento
{
    [Display(Name = "Comum")]
    Comum = 0,
    [Display(Name = "Especial")]
    Especial = 1,
    [Display(Name = "Especial - notificação branca")]
    EspecialBranca = 2,
    [Display(Name = "Especial - notificação azul")]
    EspecialAzul = 3,
    [Display(Name = "Especial - notificação amarela")]
    EspecialAmarela = 4
}
