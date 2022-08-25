using MediatR;
using Pulsar.Services.Identity.API.Application.BaseTypes;
using Pulsar.Services.Identity.Domain.Events.Usuarios;
using Pulsar.Services.Shared.Enumerations;

namespace Pulsar.Services.Identity.API.Application.DomainEvents.Usuarios
{
    public class LimparCacheUsuariosDomainEventHandler : IdentityDomainEventHandler<UsuarioModificadoDomainEvent>, INotificationHandler<UsuariosModificadosNonBasicDomainEvent>
    {
        private readonly ICacheServer _cacheServer;
        public LimparCacheUsuariosDomainEventHandler(ICacheServer cacheServer, IdentityDomainEventHandlerContext<UsuarioModificadoDomainEvent> ctx) : base(ctx)
        {
            _cacheServer = cacheServer;
        }

        public async Task Handle(UsuariosModificadosNonBasicDomainEvent evt, CancellationToken ct)
        {
            //Limpa o cache de usuários
            if (evt.Modificacao == ChangeEvent.Created)
            {
                await _cacheServer.Category(UsuarioQueries.CacheCategories.FindUsuarios).ClearAll();
            }
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
                await LimparCacheDominios(evt.UsuarioId, ct);
                await _cacheServer.Category(UsuarioQueries.CacheCategories.FindUsuarios).ClearAll();
                var key = UsuarioQueries.CacheCategories.GetBasicUserInfoKey(evt.UsuarioId.ToString()).ToCacheKey();
                await _cacheServer.Clear(key);
            }
        }

        private async Task LimparCacheDominios(ObjectId usuarioId, CancellationToken ct)
        {
            var dominiosAdministrados = (await UsuarioRepository.FindOneByIdAsync(usuarioId, ct))!.DominiosAdministrados;
            if (dominiosAdministrados.Any())
            {
                await _cacheServer.Category(DominioQueries.CacheCategories.FindDominios).ClearAll();
                foreach (var dominio in dominiosAdministrados)
                {
                    var dominioKey = DominioQueries.CacheCategories.GetDominioDetails(dominio.ToString()).ToCacheKey();
                    await _cacheServer.Clear(dominioKey);
                }
            }
        }
    }
}
