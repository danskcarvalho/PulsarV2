using Pulsar.Services.Identity.API.Application.BaseTypes;
using Pulsar.Services.Identity.Domain.Aggregates.Convites;
using Pulsar.Services.Identity.Domain.Events.Convites;

namespace Pulsar.Services.Identity.API.Application.DomainEvents.Usuarios;

public class CriarUsuarioConvidadoDEH : IdentityDomainEventHandler<UsuarioConvidadoDE>
{
    public CriarUsuarioConvidadoDEH(IdentityDomainEventHandlerContext<UsuarioConvidadoDE> ctx) : base(ctx)
    {
    }

    protected override async Task HandleAsync(UsuarioConvidadoDE evt, CancellationToken ct)
    {
        var usuarioExistente = await UsuarioRepository.FindOneByIdAsync(evt.UsuarioId, ct);
        if (usuarioExistente is not null)
            return;

        var usuario = new Usuario(
            evt.UsuarioId,
            evt.Email!,
            evt.Email,
            Guid.NewGuid().ToString("N"),
            GeneralExtensions.GetSalt(),
            GeneralExtensions.GetSalt(),
            new AuditInfo(evt.UsuarioLogadoId))
        {
            IsAtivo = false,
            IsConvitePendente = true
        };
        usuario.Criar();

        await UsuarioRepository.InsertOneAsync(usuario);
    }
}
