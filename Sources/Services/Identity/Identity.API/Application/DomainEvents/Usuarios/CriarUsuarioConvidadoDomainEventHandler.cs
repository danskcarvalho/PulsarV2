using Pulsar.Services.Identity.API.Application.BaseTypes;
using Pulsar.Services.Identity.Domain.Events.Convites;
using Pulsar.Services.Identity.Domain.Specifications;

namespace Pulsar.Services.Identity.API.Application.DomainEvents.Usuarios;

public class CriarUsuarioConvidadoDomainEventHandler : IdentityDomainEventHandler<ConviteAceitoDomainEvent>
{
    public CriarUsuarioConvidadoDomainEventHandler(ILogger<IdentityDomainEventHandler<ConviteAceitoDomainEvent>> logger, IDbSession session, IEnumerable<IIsRepository> repositories) : base(logger, session, repositories)
    {
    }

    protected override async Task HandleAsync(ConviteAceitoDomainEvent evt, CancellationToken ct)
    {
        var usuarioExistente = await UsuarioRepository.FindOneAsync(new FindUsuarioByEitherUsenameOrEmailSpec(evt.NomeUsuario, evt.Email), ct);
        if (usuarioExistente != null)
            throw new IdentityDomainException(ExceptionKey.UsuarioJaConvidado);

        var salt = GeneralExtensions.GetSalt();
        var usuario = new Usuario(evt.UsuarioId, evt.PrimeiroNome, evt.Email, evt.NomeUsuario, salt, (salt + evt.Senha).SHA256Hash(), new AuditInfo(evt.CriadoPorUsuarioId))
        {
            IsAtivo = true,
            UltimoNome = evt.Sobrenome
        };
        if (evt.AdministrarDominio)
            usuario.DominiosAdministrados.Add(evt.DominioId);
        foreach (var grp in evt.Grupos)
        {
            usuario.Grupos.Add(new UsuarioGrupo(evt.DominioId, grp.GrupoId, grp.SubGrupoId));
        }
    }
}
