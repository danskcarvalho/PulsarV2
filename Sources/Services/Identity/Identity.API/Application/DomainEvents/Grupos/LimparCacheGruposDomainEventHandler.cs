using Pulsar.BuildingBlocks.Caching.Abstractions;
using Pulsar.Services.Identity.Domain.Events.Dominios;
using Pulsar.Services.Identity.Domain.Events.Grupos;
using Pulsar.Services.Shared.Enumerations;

namespace Pulsar.Services.Identity.API.Application.DomainEvents.Grupos;

public class LimparCacheGruposDomainEventHandler : IdentityDomainEventHandler<GrupoModificadoDomainEvent>
{
    private readonly ICacheServer _cacheServer;
    public LimparCacheGruposDomainEventHandler(ICacheServer cacheServer, IdentityDomainEventHandlerContext<GrupoModificadoDomainEvent> ctx) : base(ctx)
    {
        _cacheServer = cacheServer;
    }

    protected override async Task HandleAsync(GrupoModificadoDomainEvent evt, CancellationToken ct)
    {
        await _cacheServer.Category(GrupoQueries.CacheCategories.FindGrupos).ClearAll();
    }
}
