﻿using Pulsar.Services.Shared.DTOs;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Pulsar.Services.Identity.Contracts.DTOs;

public class UsuarioDetalhesDTO
{
    [SetsRequiredMembers]
    public UsuarioDetalhesDTO(string usuarioId, string? avatarUrl, string primeiroNome, string? ultimoNome, string nomeCompleto, List<UsuarioGrupo> grupos, bool isAtivo, bool isSuperUsuario,
        List<Dominio> dominiosBloqueados, List<Dominio> dominiosAdministrados, string? email, string nomeUsuario, bool isConvitePendente, AuditInfoDTO auditInfo)
    {
        UsuarioId = usuarioId;
        AvatarUrl = avatarUrl;
        PrimeiroNome = primeiroNome;
        UltimoNome = ultimoNome;
        NomeCompleto = nomeCompleto;
        Grupos = grupos;
        IsAtivo = isAtivo;
        IsSuperUsuario = isSuperUsuario;
        DominiosBloqueados = dominiosBloqueados;
        DominiosAdministrados = dominiosAdministrados;
        Email = email;
        NomeUsuario = nomeUsuario;
        IsConvitePendente = isConvitePendente;
        AuditInfo = auditInfo;
    }

    public UsuarioDetalhesDTO()
    {
        Grupos = new List<UsuarioGrupo>();
        DominiosBloqueados = new List<Dominio>();
        DominiosAdministrados = new List<Dominio>();
    }

    /// <summary>
    /// Id do usuário.
    /// </summary>
    public required string UsuarioId { get; set; }
    /// <summary>
    /// Url da imagem utilizada como avatar do usuário.
    /// </summary>
    public string? AvatarUrl { get; set; }
    /// <summary>
    /// Primeiro nome.
    /// </summary>
    public required string PrimeiroNome { get; set; }
    /// <summary>
    /// Sobrenome.
    /// </summary>
    public string? UltimoNome { get; set; }
    /// <summary>
    /// Nome completo. Junção do primeiro nome + sobrenome.
    /// </summary>
    public required string NomeCompleto { get; set; }
    /// <summary>
    /// Grupos aos quais o usuário pertence.
    /// </summary>
    public List<UsuarioGrupo> Grupos { get; private set; }
    /// <summary>
    /// Flag global indicando se o usuário está ativo.
    /// </summary>
    public bool IsAtivo { get; set; }
    /// <summary>
    /// Apenas o usuário administrador é um super usuário.
    /// </summary>
    public bool IsSuperUsuario { get; private set; }
    /// <summary>
    /// Domínios onde o usuário está bloqueado.
    /// </summary>
    public List<Dominio> DominiosBloqueados { get; private set; }
    /// <summary>
    /// Domínios administrados por esse usuário.
    /// </summary>
    public List<Dominio> DominiosAdministrados { get; private set; }
    /// <summary>
    /// E-mail do usuário. Único.
    /// </summary>
    public string? Email { get; set; }
    /// <summary>
    /// Nome de usuário. Único.
    /// </summary>
    public required string NomeUsuario { get; set; }
    /// <summary>
    /// Se o usuário já aceito o convite para entrar no Pulsar.
    /// </summary>
    public bool IsConvitePendente { get; set; }
    /// <summary>
    /// Dados de auditoria.
    /// </summary>
    public required AuditInfoDTO AuditInfo { get; set; }

    public class Dominio
    {
        /// <summary>
        /// Id do domínio.
        /// </summary>
        public required string DominioId { get; set; }
        /// <summary>
        /// Nome do domínio.
        /// </summary>
        public required string Nome { get; set; }

        [SetsRequiredMembers]
        public Dominio(string dominioId, string nome)
        {
            DominioId = dominioId;
            Nome = nome;
        }

        public Dominio()
        {

        }
    }

    public class UsuarioGrupo
    {
        /// <summary>
        /// Id do domínio.
        /// </summary>
        public required string DominioId { get; set; }
        /// <summary>
        /// Nome do domínio.
        /// </summary>
        public required string Nome { get; set; }
        /// <summary>
        /// Id do grupo.
        /// </summary>
        public required string GrupoId { get; set; }
        /// <summary>
        /// Nome do grupo.
        /// </summary>
        public required string GrupoNome { get; set; }
        /// <summary>
        /// Id do subgrupo.
        /// </summary>
        public required string SubGrupoId { get; set; }
        /// <summary>
        /// Nome do subgrupo.
        /// </summary>
        public required string SubGrupoNome { get; set; }

        [SetsRequiredMembers]
        public UsuarioGrupo(string dominioId, string nome, string grupoId, string grupoNome, string subGrupoId, string subGrupoNome)
        {
            DominioId = dominioId;
            Nome = nome;
            GrupoId = grupoId;
            GrupoNome = grupoNome;
            SubGrupoId = subGrupoId;
            SubGrupoNome = subGrupoNome;
        }

        public UsuarioGrupo()
        {

        }
    }
}
