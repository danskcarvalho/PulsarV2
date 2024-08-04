namespace Pulsar.Services.Identity.Contracts.Enumerations;

public enum PermissoesDominio
{
    [Display(Description = "Editar Grupos")]
    EditarGrupos = 1,
    [Display(Description = "Convidar Usuário")]
    ConvidarUsuario = 2,
    [Display(Description = "Listar Usuários")]
    ListarUsuarios = 3,
    [Display(Description = "Bloquear/Desbloquear Usuários")]
    BloquearUsuarios = 4,
    [Display(Description = "Detalhar Usuários")]
    DetalharUsuarios = 3,
}
