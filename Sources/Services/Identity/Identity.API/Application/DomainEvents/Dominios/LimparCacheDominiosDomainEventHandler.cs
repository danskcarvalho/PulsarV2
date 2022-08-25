using Pulsar.Services.Identity.Domain.Events.Dominios;
using Pulsar.Services.Shared.Enumerations;

namespace Pulsar.Services.Identity.API.Application.DomainEvents.Dominios;

public class LimparCacheDominiosDomainEventHandler : IdentityDomainEventHandler<DominioModificadoDomainEvent>
{
    private readonly ICacheServer _cacheServer;
    public LimparCacheDominiosDomainEventHandler(ICacheServer cacheServer, IdentityDomainEventHandlerContext<DominioModificadoDomainEvent> ctx) : base(ctx)
    {
        _cacheServer = cacheServer;
    }

    protected override async Task HandleAsync(DominioModificadoDomainEvent evt, CancellationToken ct)
    {
        if (evt.Modificacao == ChangeEvent.Created)
        {
            await _cacheServer.Category(DominioQueries.CacheCategories.FindDominios).ClearAll();
        }
        else if (evt.DetalhesModificacao.HasBasic())
        {
            await _cacheServer.Category(DominioQueries.CacheCategories.FindDominios).ClearAll();
            var key = DominioQueries.CacheCategories.GetDominioDetails(evt.DominioId.ToString()).ToCacheKey();
            await _cacheServer.Clear(key);
        }
    }
}
