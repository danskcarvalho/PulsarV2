namespace Pulsar.Services.Identity.Contracts.Enumerations;

public enum ExceptionKey
{
    [Display(Description = "Usuário não encontrado.")]
    UsuarioNaoEncontrado = 1,
    [Display(Description = "Usuário não possui e-mail cadastrado.")]
    UsuarioSemEmail = 2,
    [Display(Description = "Token para a mudança de senha expirado.")]
    TokenMudancaSenhaExpirado = 3,
    [Display(Description = "Token para a mudança de senha inválido.")]
    TokenMudancaSenhaInvalido = 4,
    [Display(Description = "Convite não encontrado.")]
    ConviteNaoEncontrado = 5,
    [Display(Description = "Convite expirado.")]
    ConviteExpirado = 6,
    [Display(Description = "Este convite já foi aceito.")]
    ConviteJaAceito = 7,
    [Display(Description = "Já existe um usuário para o e-mail informado neste convite.")]
    UsuarioJaConvidado = 8, 
    [Display(Description = "Token inválido.")]
    ConviteTokenInvalido = 9,
    [Display(Description = "Usuário foi convidado para administrar o domínio mas o domínio já tem administrador ou está inativo.")]
    ConviteDominioInvalido = 10,
    [Display(Description = "Convite Inválido.")]
    ConviteInvalido = 11,
    [Display(Description = "Já existe um usuário com o nome de usuário informado.")]
    NomeUsuarioNaoUnico = 12,
    [Display(Description = "Convite para este usuário ainda não foi aceito.")]
    ConviteNaoAceito = 13,
    [Display(Description = "Senha atual inválida.")]
    SenhaAtualInvalida = 14,
    [Display(Description = "O usuário 'administrador' não pode ser bloqueado/desbloqueado.")]
    SuperUsuarioNaoPodeSerBloqueado = 15,
    [Display(Description = "O usuário 'administrador' não pode administrar um domínio.")]
    SuperUsuarioNaoPodeAdministrarDominio = 16,
    [Display(Description = "Domínio não encontrado.")]
    DominioNaoEncontrado = 17,
    [Display(Description = "O usuário informado para administrar este domínio está bloqueado nele.")]
    UsuarioAdministradorIsBloqueadoDominio = 18,
    [Display(Description = "O usuário administrador não pode ser bloqueado neste domínio.")]
    UsuarioAdministradorNaoPodeSerBloqueadoDominio = 19,
    [Display(Description = "O usuário 'administrador' não pode ser bloqueado ou desbloqueado dentro deste domínio.")]
    SuperUsuarioNaoPodeSerBloqueadoDominio = 20,
    [Display(Description = "Grupo não encontrado.")]
    GrupoNaoEncontrado = 21,
    [Display(Description = "Subgrupo com o nome informado já existe.")]
    SubgrupoJaExistente = 22,
    [Display(Description = "Subgrupo não encontrado.")]
    SubgrupoNaoEncontrado = 23,
    [Display(Description = "O usuário 'administrador' não pode ser adicionado ou removido de um grupo.")]
    SuperUsuarioNaoPodeserAdicionadoEmGrupo = 24,
    [Display(Description = "Você não está logado em um domínio.")]
    DominioNaoLogado = 25
}
