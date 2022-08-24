using Pulsar.Services.Identity.API.Application.BaseTypes;
using Pulsar.Services.Identity.Domain.Events.Usuarios;
using Pulsar.Services.Shared.Enumerations;

namespace Pulsar.Services.Identity.API.Application.DomainEvents.Usuarios
{
    public class LimparCacheUsuariosDomainEventHandler : IdentityDomainEventHandler<UsuarioModificadoDomainEvent>
    {
        private readonly ICacheServer _cacheServer;
        public LimparCacheUsuariosDomainEventHandler(ICacheServer cacheServer, IdentityDomainEventHandlerContext<UsuarioModificadoDomainEvent> ctx) : base(ctx)
        {
            _cacheServer = cacheServer;
        }

        protected override async Task HandleAsync(UsuarioModificadoDomainEvent evt, CancellationToken ct)
        {
            //Limpa o cache de usuários
            if (evt.Modificacao == ChangeEvent.Created)
            {
                await _cacheServer.Category(UsuarioQueries.CacheCategories.FindUsuarios).ClearAll();
            }
            else if (evt.DetalhesModificacao.HasBasic())
            {
                await _cacheServer.Category(UsuarioQueries.CacheCategories.FindUsuarios).ClearAll();
                var key = UsuarioQueries.CacheCategories.GetBasicUserInfoKey(evt.UsuarioId.ToString()).ToCacheKey();
                await _cacheServer.Clear(key);
            }
        }
    }
}
