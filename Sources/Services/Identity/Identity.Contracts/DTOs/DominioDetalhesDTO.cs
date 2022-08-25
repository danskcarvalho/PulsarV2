namespace Pulsar.Services.Identity.Contracts.DTOs;

public class DominioDetalhesDTO
{
    public DominioDetalhesDTO(string dominioId, string nome, bool isAtivo, UsuarioAdmin? usuarioAdministrador)
    {
        DominioId = dominioId;
        Nome = nome;
        IsAtivo = isAtivo;
        UsuarioAdministrador = usuarioAdministrador;
    }

    /// <summary>
    /// Id do domínio.
    /// </summary>
    public string DominioId { get; set; }
    /// <summary>
    /// Nome do domínio.
    /// </summary>
    public string Nome { get; set; }
    /// <summary>
    /// true se o domínio estiver ativo.
    /// </summary>
    public bool IsAtivo { get; set; }
    /// <summary>
    /// Usuário que administra o domínio (se houve).
    /// </summary>
    public UsuarioAdmin? UsuarioAdministrador { get; set; }

    public class UsuarioAdmin
    {
        /// <summary>
        /// Id do usuário.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Nome completo do usuário.
        /// </summary>
        public string Nome { get; set; }
        /// <summary>
        /// E-mail do usuário.
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// Nome de usuário.
        /// </summary>
        public string NomeUsuario { get; set; }

        public UsuarioAdmin(string id, string nome, string email, string nomeUsuario)
        {
            Id = id;
            Nome = nome;
            Email = email;
            NomeUsuario = nomeUsuario;
        }
    }
}
