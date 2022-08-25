using System;

namespace Pulsar.Services.Identity.Contracts.DTOs;

public class DominioListadoDTO
{
    public DominioListadoDTO(string dominioId, string nome, bool isAtivo, UsuarioAdmin? usuarioAdministrador)
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

public class CursorDominioListadoDTO
{
    public string LastNome { get; set; }
    public string LastDominioId { get; set; }
    public string? Filtro { get; set; }

    public CursorDominioListadoDTO(string lastNome, string lastDominioId, string? filtro)
    {
        LastNome = lastNome;
        LastDominioId = lastDominioId;
        Filtro = filtro;
    }

    public CursorDominioListadoDTO? Next(List<DominioListadoDTO> dominios)
    {
        if (dominios == null || dominios.Count == 0)
            return null;

        return new CursorDominioListadoDTO(dominios.Last().Nome, dominios.Last().DominioId, this.Filtro);
    }

    public static CursorDominioListadoDTO? Next(List<DominioListadoDTO> dominios, string? filtro)
    {
        if (dominios == null || dominios.Count == 0)
            return null;

        return new CursorDominioListadoDTO(dominios.Last().Nome, dominios.Last().DominioId, filtro);
    }
}
