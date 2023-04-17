using Microsoft.Extensions.Logging;
using Pulsar.BuildingBlocks.EventBus.Abstractions;

namespace Pulsar.Services.Identity.Functions.Application.BaseTypes;

public abstract class IdentityCommandHandler<TRequest> : CommandHandler<TRequest> where TRequest : IRequest
{
    protected IConviteRepository ConviteRepository { get; }
    protected IDominioRepository DominioRepository { get; }
    protected IEstabelecimentoRepository EstabelecimentoRepository { get; }
    protected IGrupoRepository GrupoRepository { get; }
    protected IRedeEstabelecimentosRepository RedeEstabelecimentosRepository { get; }
    protected IUsuarioRepository UsuarioRepository { get; }
    protected ILogger Logger { get; }
    public ISaveIntegrationEventLog EventLog { get; }

    protected IdentityCommandHandler(IdentityCommandHandlerContext<TRequest> ctx) : base(ctx.Session)
    {
        ConviteRepository = (IConviteRepository)ctx.Repositories.First(r => r is IConviteRepository);
        DominioRepository = (IDominioRepository)ctx.Repositories.First(r => r is IDominioRepository);
        EstabelecimentoRepository = (IEstabelecimentoRepository)ctx.Repositories.First(r => r is IEstabelecimentoRepository);
        GrupoRepository = (IGrupoRepository)ctx.Repositories.First(r => r is IGrupoRepository);
        RedeEstabelecimentosRepository = (IRedeEstabelecimentosRepository)ctx.Repositories.First(r => r is IRedeEstabelecimentosRepository);
        UsuarioRepository = (IUsuarioRepository)ctx.Repositories.First(r => r is IUsuarioRepository);
        Logger = ctx.Logger;
        EventLog = ctx.EventLog;
    }
}

public abstract class IdentityCommandHandler<TRequest, TResponse> : CommandHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    protected IConviteRepository ConviteRepository { get; }
    protected IDominioRepository DominioRepository { get; }
    protected IEstabelecimentoRepository EstabelecimentoRepository { get; }
    protected IGrupoRepository GrupoRepository { get; }
    protected IRedeEstabelecimentosRepository RedeEstabelecimentosRepository { get; }
    protected IUsuarioRepository UsuarioRepository { get; }
    protected ILogger Logger { get; }
    public ISaveIntegrationEventLog EventLog { get; }

    protected IdentityCommandHandler(IdentityCommandHandlerContext<TRequest, TResponse> ctx) : base(ctx.Session)
    {
        ConviteRepository = (IConviteRepository)ctx.Repositories.First(r => r is IConviteRepository);
        DominioRepository = (IDominioRepository)ctx.Repositories.First(r => r is IDominioRepository);
        EstabelecimentoRepository = (IEstabelecimentoRepository)ctx.Repositories.First(r => r is IEstabelecimentoRepository);
        GrupoRepository = (IGrupoRepository)ctx.Repositories.First(r => r is IGrupoRepository);
        RedeEstabelecimentosRepository = (IRedeEstabelecimentosRepository)ctx.Repositories.First(r => r is IRedeEstabelecimentosRepository);
        UsuarioRepository = (IUsuarioRepository)ctx.Repositories.First(r => r is IUsuarioRepository);
        Logger = ctx.Logger;
        EventLog = ctx.EventLog;
    }
}

public class IdentityCommandHandlerContext<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    public ILogger<IdentityCommandHandler<TRequest, TResponse>> Logger { get; }
    public IDbSession Session { get; }
    public IEnumerable<IIsRepository> Repositories { get; }
    public ISaveIntegrationEventLog EventLog { get; }

    public IdentityCommandHandlerContext(ILogger<IdentityCommandHandler<TRequest, TResponse>> logger, IDbSession session, IEnumerable<IIsRepository> repositories, ISaveIntegrationEventLog eventLog)
    {
        Logger = logger;
        Session = session;
        Repositories = repositories;
        EventLog = eventLog;
    }
}

public class IdentityCommandHandlerContext<TEvent> where TEvent : IRequest
{
    public ILogger<IdentityCommandHandler<TEvent>> Logger { get; }
    public IDbSession Session { get; }
    public IEnumerable<IIsRepository> Repositories { get; }
    public ISaveIntegrationEventLog EventLog { get; }

    public IdentityCommandHandlerContext(ILogger<IdentityCommandHandler<TEvent>> logger, IDbSession session, IEnumerable<IIsRepository> repositories, ISaveIntegrationEventLog eventLog)
    {
        Logger = logger;
        Session = session;
        Repositories = repositories;
        EventLog = eventLog;
    }
}
