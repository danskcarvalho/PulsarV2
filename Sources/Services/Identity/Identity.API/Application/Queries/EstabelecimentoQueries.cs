using Pulsar.BuildingBlocks.DDD.Mongo;

namespace Pulsar.Services.Identity.API.Application.Queries;

public class EstabelecimentoQueries : IdentityQueries, IEstabelecimentoQueries
{
    public EstabelecimentoQueries(IdentityQueriesContext ctx) : base(ctx)
    {
    }

    public async Task<EstabelecimentoLogadoDTO> GetEstabelecimentoLogado(string estabelecimentoId)
    {
        var id = estabelecimentoId.ToObjectId();
        var e = await EstabelecimentosCollection.FindAsync(e => e.Id == id).FirstOrDefaultAsync();
        if (e == null)
            throw new IdentityDomainException(ExceptionKey.EstabelecimentoNaoEncontrado);
        return new EstabelecimentoLogadoDTO(e.Id.ToString(), e.Nome);
    }
}
