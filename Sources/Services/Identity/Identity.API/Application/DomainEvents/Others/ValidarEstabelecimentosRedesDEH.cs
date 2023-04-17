using Pulsar.Services.Identity.Domain.Events.Grupos;
using Pulsar.Services.Identity.Domain.Specifications;

namespace Pulsar.Services.Identity.API.Application.DomainEvents.Others;

public class ValidarEstabelecimentosRedesDEH : IdentityDomainEventHandler<GrupoModificadoDE>
{
    public ValidarEstabelecimentosRedesDEH(IdentityDomainEventHandlerContext<GrupoModificadoDE> ctx) : base(ctx)
    {
    }

    protected override async Task HandleAsync(GrupoModificadoDE evt, CancellationToken ct)
    {
        if (evt.SubGruposAdicionados.Count != 0)
        {
            var estabelecimentos = evt.SubGruposAdicionados.SelectMany(sg => sg.PermissoesEstabelecimentos).Where(ep => ep.Seletor.EstabelecimentoId.HasValue).Select(ep => ep.Seletor.EstabelecimentoId!.Value).ToList();
            var redes = evt.SubGruposAdicionados.SelectMany(sg => sg.PermissoesEstabelecimentos).Where(ep => ep.Seletor.RedeEstabelecimentoId.HasValue).Select(ep => ep.Seletor.RedeEstabelecimentoId!.Value).ToList();
            if (estabelecimentos.Count != 0)
            {
                var b = await EstabelecimentoRepository.AllExistsAsync(estabelecimentos, new FindEstabelecimentosDominioSpec(evt.DominioId));
                if (!b)
                    throw new IdentityDomainException(ExceptionKey.EstabelecimentoNaoEncontrado);
            }
            if (redes.Count != 0)
            {
                var b = await RedeEstabelecimentosRepository.AllExistsAsync(redes, new FindRedesEstabelecimentosDominioSpec(evt.DominioId));
                if (!b)
                    throw new IdentityDomainException(ExceptionKey.RedeEstabelecimentosNaoEncontrado);
            }
        }
        if (evt.SubGruposModificados.Count != 0)
        {
            var estabelecimentos = evt.SubGruposModificados.SelectMany(sg => sg.PermissoesEstabelecimentos).Where(ep => ep.Seletor.EstabelecimentoId.HasValue).Select(ep => ep.Seletor.EstabelecimentoId!.Value).ToList();
            var redes = evt.SubGruposModificados.SelectMany(sg => sg.PermissoesEstabelecimentos).Where(ep => ep.Seletor.RedeEstabelecimentoId.HasValue).Select(ep => ep.Seletor.RedeEstabelecimentoId!.Value).ToList();
            if (estabelecimentos.Count != 0)
            {
                var b = await EstabelecimentoRepository.AllExistsAsync(estabelecimentos, new FindEstabelecimentosDominioSpec(evt.DominioId));
                if (!b)
                    throw new IdentityDomainException(ExceptionKey.EstabelecimentoNaoEncontrado);
            }
            if (redes.Count != 0)
            {
                var b = await RedeEstabelecimentosRepository.AllExistsAsync(redes, new FindRedesEstabelecimentosDominioSpec(evt.DominioId));
                if (!b)
                    throw new IdentityDomainException(ExceptionKey.RedeEstabelecimentosNaoEncontrado);
            }
        }
    }
}
