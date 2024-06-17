using Pulsar.BuildingBlocks.Sync.Contracts;
using Pulsar.Services.Facility.Contracts.Shadows;
using Pulsar.Services.Identity.Domain.Aggregates.Grupos;

namespace Pulsar.Services.Identity.API.Application.BaseTypes;

public class IdentityQueries : QueryHandler
{
    public IdentityQueries(IdentityQueriesContext ctx) : base(ctx.Factory, ctx.ClusterName)
    {
        UsuariosCollection = GetCollection<Usuario>(Constants.CollectionNames.USUARIOS);
        DominiosCollection = GetCollection<Dominio>(Constants.CollectionNames.DOMINIOS);
        ConvitesCollection = GetCollection<Convite>(Constants.CollectionNames.CONVITES);
        GruposCollection = GetCollection<Grupo>(Constants.CollectionNames.GRUPOS);
        EstabelecimentosCollection = GetCollection<EstabelecimentoShadow>(Shadow.GetCollectionName<EstabelecimentoShadow>());
        RedesEstabelecimentosCollection = GetCollection<RedeEstabelecimentosShadow>(Shadow.GetCollectionName<RedeEstabelecimentosShadow>());
    }

    protected IMongoCollection<Usuario> UsuariosCollection { get; private set; }
    protected IMongoCollection<Dominio> DominiosCollection { get; private set; }
    protected IMongoCollection<Convite> ConvitesCollection { get; private set; }
    protected IMongoCollection<Grupo> GruposCollection { get; private set; }
    protected IMongoCollection<EstabelecimentoShadow> EstabelecimentosCollection { get; private set; }
    protected IMongoCollection<RedeEstabelecimentosShadow> RedesEstabelecimentosCollection { get; private set; }
}
