﻿using Pulsar.BuildingBlocks.DDD.Abstractions;
using Pulsar.BuildingBlocks.DDD.Contexts;
using Pulsar.BuildingBlocks.EventBus.Abstractions;
using Pulsar.Services.Facility.Contracts.Shadows;

namespace Pulsar.Services.Identity.API.Application.BaseTypes;

public abstract class IdentityCommandHandler<TRequest> : CommandHandler<TRequest> where TRequest : IRequest
{
    protected IConviteRepository ConviteRepository { get; }
    protected IDominioRepository DominioRepository { get; }
    protected IShadowRepository<EstabelecimentoShadow> EstabelecimentoRepository { get; }
    protected IGrupoRepository GrupoRepository { get; }
    protected IShadowRepository<RedeEstabelecimentosShadow> RedeEstabelecimentosRepository { get; }
    protected IUsuarioRepository UsuarioRepository { get; }
    protected ILogger Logger { get; }
    public ISaveIntegrationEventLog EventLog { get; }

    protected IdentityCommandHandler(IdentityCommandHandlerContext<TRequest> ctx) : base(ctx.Session, ctx.DbContextFactory)
    {
        ConviteRepository = (IConviteRepository)ctx.Repositories.First(r => r is IConviteRepository);
        DominioRepository = (IDominioRepository)ctx.Repositories.First(r => r is IDominioRepository);
        EstabelecimentoRepository = (IShadowRepository<EstabelecimentoShadow>)ctx.Repositories.First(r => r is IShadowRepository<EstabelecimentoShadow>);
        GrupoRepository = (IGrupoRepository)ctx.Repositories.First(r => r is IGrupoRepository);
        RedeEstabelecimentosRepository = (IShadowRepository<RedeEstabelecimentosShadow>)ctx.Repositories.First(r => r is IShadowRepository<RedeEstabelecimentosShadow>);
        UsuarioRepository = (IUsuarioRepository)ctx.Repositories.First(r => r is IUsuarioRepository);
        Logger = ctx.Logger;
        EventLog = ctx.EventLog;
    }
}

public abstract class IdentityCommandHandler<TRequest, TResponse> : CommandHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    protected IConviteRepository ConviteRepository { get; }
    protected IDominioRepository DominioRepository { get; }
    protected IShadowRepository<EstabelecimentoShadow> EstabelecimentoRepository { get; }
    protected IGrupoRepository GrupoRepository { get; }
    protected IShadowRepository<RedeEstabelecimentosShadow> RedeEstabelecimentosRepository { get; }
    protected IUsuarioRepository UsuarioRepository { get; }
    protected ILogger Logger { get; }
    public ISaveIntegrationEventLog EventLog { get; }

    protected IdentityCommandHandler(IdentityCommandHandlerContext<TRequest, TResponse> ctx) : base(ctx.Session, ctx.DbContextFactory)
    {
        ConviteRepository = (IConviteRepository)ctx.Repositories.First(r => r is IConviteRepository);
        DominioRepository = (IDominioRepository)ctx.Repositories.First(r => r is IDominioRepository);
        EstabelecimentoRepository = (IShadowRepository<EstabelecimentoShadow>)ctx.Repositories.First(r => r is IShadowRepository<EstabelecimentoShadow>);
        GrupoRepository = (IGrupoRepository)ctx.Repositories.First(r => r is IGrupoRepository);
        RedeEstabelecimentosRepository = (IShadowRepository<RedeEstabelecimentosShadow>)ctx.Repositories.First(r => r is IShadowRepository<RedeEstabelecimentosShadow>);
        UsuarioRepository = (IUsuarioRepository)ctx.Repositories.First(r => r is IUsuarioRepository);
        Logger = ctx.Logger;
        EventLog = ctx.EventLog;
    }
}

public class IdentityCommandHandlerContext<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    public ILogger<IdentityCommandHandler<TRequest, TResponse>> Logger { get; }
    public IDbSession Session { get; }
    public DbContextFactory DbContextFactory { get; }
    public IEnumerable<IIsRepository> Repositories { get; }
    public ISaveIntegrationEventLog EventLog { get; }

    public IdentityCommandHandlerContext(ILogger<IdentityCommandHandler<TRequest, TResponse>> logger, IDbSession session, DbContextFactory contextFactory, IEnumerable<IIsRepository> repositories, ISaveIntegrationEventLog eventLog)
    {
        Logger = logger;
        Session = session;
        DbContextFactory = contextFactory;
        Repositories = repositories;
        EventLog = eventLog;
    }
}

public class IdentityCommandHandlerContext<TEvent> where TEvent : IRequest
{
    public ILogger<IdentityCommandHandler<TEvent>> Logger { get; }
    public IDbSession Session { get; }
    public DbContextFactory DbContextFactory { get; }
    public IEnumerable<IIsRepository> Repositories { get; }
    public ISaveIntegrationEventLog EventLog { get; }

    public IdentityCommandHandlerContext(ILogger<IdentityCommandHandler<TEvent>> logger, IDbSession session, DbContextFactory contextFactory, IEnumerable<IIsRepository> repositories, ISaveIntegrationEventLog eventLog)
    {
        Logger = logger;
        Session = session;
        DbContextFactory = contextFactory;
        Repositories = repositories;
        EventLog = eventLog;
    }
}
