using Pulsar.Services.Identity.Contracts.Commands.Convites;

namespace Pulsar.Services.Identity.UI.Clients.Interfaces;

public interface IAceitarConviteClient
{
    Task Aceitar(AceitarConviteCmd cmd, CancellationToken ct = default);
}
