using FluentValidation;
using Pulsar.Services.Identity.Contracts.Utils;
using Pulsar.Services.Shared.Attributes;
using System.Text.Json.Serialization;

namespace Pulsar.Services.Identity.Contracts.Commands.Grupos;

public class CriarSubGrupoCommand : IRequest<CreatedCommandResult>
{
    [SwaggerExclude]
    public string? UsuarioLogadoId { get; set; }
    [SwaggerExclude]
    public string? DominioId { get; set; }
    [SwaggerExclude]
    public string? GrupoId { get; set; }
    /// <summary>
    /// Nome do subgrupo.
    /// </summary>
    public string? Nome { get; set; }
    /// <summary>
    /// Permissões de domínio.
    /// </summary>
    public List<PermissoesDominio>? PermissoesDominio { get; set; }
    /// <summary>
    /// Permissões de estabelecimento.
    /// </summary>
    public List<PermissoesEstabelecimentoOuRede>? PermissoesEstabelecimento { get; set; }


    public class PermissoesEstabelecimentoOuRede
    {
        /// <summary>
        /// Aplicar apenas a este estabelecimento. Não pode ser informado se for informada uma rede de estabelecimentos.
        /// </summary>
        public string? EstabelecimentoId { get; set; }
        /// <summary>
        /// Aplicar a todos os estabelecimentos da rede. Não pode ser informada se for informado um estabelecimento.
        /// </summary>
        public string? RedeEstabelecimentosId { get; set; }
        /// <summary>
        /// Permissões de estabelecimento.
        /// </summary>
        public List<PermissoesEstabelecimento>? Permissoes { get; set; }
    }
}

public class CriarSubGrupoCommandValidator : AbstractValidator<CriarSubGrupoCommand>
{
    public CriarSubGrupoCommandValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("O nome é de preenchimento obrigatório.")
            .Length(3, 64).WithMessage("O nome deve ter entre 3 e 64 caracteres.");
        RuleFor(x => x.PermissoesDominio).NotNull().WithMessage("PermissoesDominio não pode ser null.");
        RuleForEach(x => x.PermissoesDominio).IsInEnum().WithMessage("Permissão de domínio inválida.");
        RuleFor(x => x.PermissoesEstabelecimento).NotNull().WithMessage("PermissoesEstabelecimento não pode ser null.");
        RuleForEach(x => x.PermissoesEstabelecimento).ChildRules(v =>
        {
            v.RuleFor(x => x.EstabelecimentoId).NotEmpty().When(x => x.RedeEstabelecimentosId == null).WithMessage("EstabelecimentoId é de preenchimento obrigatório.");
            v.RuleFor(x => x.EstabelecimentoId).Null().When(x => x.RedeEstabelecimentosId != null).WithMessage("EstabelecimentoId não pode ser preenchido.");
            v.RuleFor(x => x.RedeEstabelecimentosId).NotEmpty().When(x => x.EstabelecimentoId == null).WithMessage("RedeEstabelecimentosId é de preenchimento obrigatório.");
            v.RuleFor(x => x.RedeEstabelecimentosId).Null().When(x => x.EstabelecimentoId != null).WithMessage("RedeEstabelecimentosId não pode ser preenchido.");
            v.RuleFor(x => x.Permissoes).NotNull().WithMessage("Permissoes não pode ser null.");
            v.RuleForEach(x => x.Permissoes).IsInEnum().WithMessage("Permissão de estabelecimento inválida.");
        });
    }
}
