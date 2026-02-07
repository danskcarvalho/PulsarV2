# Pulsar DDD building blocks (user guide)

This repo contains three NuGet packages that let a service use a consistent DDD stack on MongoDB:

- `DDD.Contracts` - domain-only primitives such as `AggregateRoot`, `ValueObject`, `IAggregateRoot`, and `SyncPendingKey`.
- `DDD` - infrastructure abstractions and base types (`CommandHandler`, `DomainEventHandler`, repository interfaces, consistency/transaction helpers, `AuditInfo`).
- `DDD.Mongo` - MongoDB implementations (session/transaction handling, repositories, MediatR behaviors, DI helpers).

All services in this solution follow the same recipe: model your aggregate in the domain project, implement the repository in the infrastructure project, wire `AddMongoDB` in the API/worker, and handle commands/queries with MediatR using the provided base classes.

## Quick start

1. **Add package references**

   Domain project:

   ```xml
   <PackageReference Include="DDD.Contracts" />
   ```

   API/worker/infrastructure project:

   ```xml
   <PackageReference Include="DDD" />
   <PackageReference Include="DDD.Mongo" />
   ```

2. **Configure MongoDB and DDD in `appsettings.json`**

   ```json
   {
     "ConnectionStrings": {
       "Mongo": "mongodb://localhost:27017"
     },
     "MongoDB": {
       "ConnectionStringName": "Mongo",
       "Database": "pulsar-dev",
       "ClusterName": "pulsar"
     }
   }
   ```

3. **Register the stack in `Program.cs`** (from `Services/Identity/Identity.API/Program.cs`):

   ```csharp
   using Pulsar.BuildingBlocks.DDD.Mongo;
   using Pulsar.Services.Identity.API.Persistence.Repositories;
   using Pulsar.Services.Facility.Contracts.Shadows;
   using Pulsar.Services.Identity.Contracts.Commands.Usuarios;

   var builder = WebApplication.CreateBuilder(args);

   builder.Services.AddMongoDB(
       // scan repositories
       typeof(UsuarioMongoRepository).Assembly,
       // scan shadows used as read models
       typeof(EstabelecimentoShadow).Assembly);

   builder.Services.AddValidators(typeof(EsqueciMinhaSenhaCmd).Assembly);
   builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
   ```

`AddMongoDB` discovers `MongoRepository<,>` implementations and registers `IRepository` interfaces, `MongoDbSessionFactory`, MediatR pipeline behaviors (`LoggingBehavior`, `ValidatorBehavior`), shadow repositories, and query handlers.

## Model your domain

### Aggregates and value objects

Use `AggregateRoot` from `DDD.Contracts`. The `Session` aggregate in Push Notification is a real example:

```csharp
using DDD.Contracts;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Pulsar.BuildingBlocks.DDD;
using Pulsar.Services.PushNotification.Domain.Events.PushNotifications;

namespace Pulsar.Services.PushNotification.Domain.Aggregates.Sessions;

public partial class Session : AggregateRoot
{
    public static readonly TimeSpan SESSION_DURATION = TimeSpan.FromMinutes(5);

    [BsonConstructor]
    public Session(ObjectId id, ObjectId usuarioId, ObjectId? dominioId, ObjectId? estabelecimentoId, string token) : base(id)
    {
        UsuarioId = usuarioId;
        DominioId = dominioId;
        EstabelecimentoId = estabelecimentoId;
        CreatedOn = DateTime.UtcNow;
        ExpiresOn = CreatedOn.Add(SESSION_DURATION);
        Token = token;
    }

    public ObjectId UsuarioId { get; private set; }
    public ObjectId? DominioId { get; private set; }
    public ObjectId? EstabelecimentoId { get; private set; }
    public string Token { get; private set; }
    public DateTime ExpiresOn { get; private set; }
    public DateTime CreatedOn { get; private set; }

    public void Criar()
    {
        AddDomainEvent(new SessaoCriadaDE(Id, UsuarioId, DominioId, EstabelecimentoId));
    }
}
```

Value objects derive from `ValueObject`; `AuditInfo` is provided as a ready-to-use value object for tracking changes.

### Domain events

Any `INotification` you add through `AddDomainEvent` will be dispatched after the repository operation succeeds. Example event used above:

```csharp
using MediatR;
using MongoDB.Bson;

namespace Pulsar.Services.PushNotification.Domain.Events.PushNotifications;

public class SessaoCriadaDE : INotification
{
    public SessaoCriadaDE(ObjectId sessionId, ObjectId usuarioId, ObjectId? dominioId, ObjectId? estabelecimentoId)
    {
        SessionId = sessionId;
        UsuarioId = usuarioId;
        DominioId = dominioId;
        EstabelecimentoId = estabelecimentoId;
    }

    public ObjectId SessionId { get; set; }
    public ObjectId UsuarioId { get; set; }
    public ObjectId? DominioId { get; set; }
    public ObjectId? EstabelecimentoId { get; set; }
}
```

You can handle these with `DomainEventHandler<TEvent>` in your API/worker project if you need side effects.

## Repositories and specifications

Define a repository interface in the domain layer and implement it with `MongoRepository` in infrastructure. This is exactly how the Session aggregate is persisted:

```csharp
// Domain (Services/PushNotification/PushNotification.Domain/Aggregates/Sessions/ISessionRepository.cs)
using Pulsar.BuildingBlocks.DDD.Abstractions;

namespace Pulsar.Services.PushNotification.Domain.Aggregates.Sessions;

public interface ISessionRepository : IRepository<ISessionRepository, Session>
{
}
```

```csharp
// Infrastructure (Services/PushNotification/PushNotification.Infrastructure/Repositories/SessionMongoRepository.cs)
using Pulsar.BuildingBlocks.DDD.Mongo;
using Pulsar.BuildingBlocks.DDD.Mongo.Implementations;
using Pulsar.Services.PushNotification.Domain.Aggregates.Sessions;

namespace Pulsar.Services.PushNotification.Infrastructure.Repositories;

public class SessionMongoRepository : MongoRepository<ISessionRepository, Session>, ISessionRepository
{
    public SessionMongoRepository(MongoDbSession? session, MongoDbSessionFactory sessionFactory) : base(session, sessionFactory)
    {
    }

    protected override string CollectionName => Constants.CollectionNames.SESSIONS;

    protected override ISessionRepository Clone(MongoDbSession? session, MongoDbSessionFactory sessionFactory)
    {
        return new SessionMongoRepository(session, sessionFactory);
    }
}
```

Use specifications to keep queries reusable and testable. A real spec used by the session workflow:

```csharp
using Pulsar.Services.PushNotification.Domain.Aggregates.Sessions;
using Pulsar.BuildingBlocks.DDD.Abstractions;

namespace Pulsar.Services.PushNotification.Domain.Specifications.Sessions;

public class GetSessionByTokenSpec : IFindSpecification<Session>
{
    public GetSessionByTokenSpec(string token)
    {
        Token = token;
    }

    public string Token { get; }

    public FindSpecification<Session> GetSpec()
    {
        return Find.Where<Session>(s => s.Token == Token && DateTime.UtcNow < s.ExpiresOn)
            .Limit(1)
            .Build();
    }
}
```

Example usage inside an application service or handler:

```csharp
var spec = new GetSessionByTokenSpec(token);
var session = await _sessionRepository.FindOneAsync(spec, ct);
if (session is null) throw new DomainException("Session.NotFound");
```

### Update specifications

`UpdateSpecificationBuilder` lets you express MongoDB updates without writing raw driver calls. The builder supports `Set`, `Unset`, `Inc`, `Push`, `AddToSet`, `Pull`, `ForEach`, etc. Prefer creating named classes that implement `IUpdateSpecification<T>` so update logic stays reusable and testable.

Reusable spec class example:

```csharp
using Pulsar.BuildingBlocks.DDD.Abstractions;
using Pulsar.Services.PushNotification.Domain.Aggregates.Sessions;

public class ExtendSessionSpec : IUpdateSpecification<Session>
{
    private readonly ObjectId _sessionId;
    private readonly DateTime _newExpiration;

    public ExtendSessionSpec(ObjectId sessionId, DateTime newExpiration)
    {
        _sessionId = sessionId;
        _newExpiration = newExpiration;
    }

    public UpdateSpecification<Session> GetSpec()
    {
        return Update.Where<Session>(s => s.Id == _sessionId)
            .Set(s => s.ExpiresOn, _newExpiration)
            .Build();
    }
}
```

### Delete specifications

`DeleteSpecificationBuilder` keeps delete filters reusable. Create dedicated classes implementing `IDeleteSpecification<T>`.

Reusable spec class example:

```csharp
using Pulsar.BuildingBlocks.DDD.Abstractions;
using Pulsar.Services.PushNotification.Domain.Aggregates.Sessions;

public class DeleteExpiredSessionsSpec : IDeleteSpecification<Session>
{
    public DeleteSpecification<Session> GetSpec()
    {
        return Delete.Where<Session>(s => s.ExpiresOn < DateTime.UtcNow).Build();
    }
}
```

Bulk delete example:

```csharp
var spec = new DeleteExpiredSessionsSpec();
await _sessionRepository.DeleteManyAsync(spec, ct);
```

### Shadows and shadow repositories

Shadows are denormalized read models that live alongside your contracts (for example, `Services.Facility.Contracts.Shadows.EstabelecimentoShadow`). They inherit from `Shadow<T>` in `Pulsar.BuildingBlocks.Sync.Contracts` and are decorated with `[Shadow("<scope>:<name>")]` so the synchronization pipeline knows how to materialize them from integration events. Each shadow captures a curated subset of an aggregate root that needs to stay in sync across microservices; the sync machinery publishes integration events whenever the source aggregate changes and downstream services consume those events to keep their local shadows current. Because other services reference these types directly, always define them in the service’s Contracts project where they can be shared safely.

- Real example (`Services/Facility/Facility.Contracts/Shadows/EstabelecimentoShadow.cs`):

```csharp
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Pulsar.BuildingBlocks.Sync.Contracts;

namespace Pulsar.Services.Facility.Contracts.Shadows;

[Shadow("Facility:Estabelecimentos")]
public partial class EstabelecimentoShadow : Shadow<EstabelecimentoShadow>
{
    public ObjectId DominioId { get; private set; }
    public string Nome { get; set; }
    public string Cnes { get; set; }
    public List<ObjectId> Redes { get; private set; }
    public bool IsAtivo { get; set; }
    public AuditInfoShadow AuditInfo { get; set; }

    [JsonConstructor, BsonConstructor]
    public EstabelecimentoShadow(
        ObjectId id,
        ObjectId dominioId,
        string nome,
        string cnes,
        List<ObjectId> redes,
        bool isAtivo,
        AuditInfoShadow auditInfo) : base(id)
    {
        DominioId = dominioId;
        Nome = nome;
        Cnes = cnes;
        Redes = redes;
        IsAtivo = isAtivo;
        AuditInfo = auditInfo;
    }
}
```

- When you call `AddMongoDB`, pass the assemblies that contain your shadow types. The bootstrapper discovers every `Shadow<T>` and automatically registers an `IShadowRepository<T>` in DI.
- `IShadowRepository<T>` implements the same repository abstractions exposed to aggregates, so you use the usual find/update/delete specifications to query shadows without duplicating infrastructure code.
- Shadow repositories run outside the aggregate’s transactional context by default, giving handlers and background workers a cheap way to read the latest projections while commands keep working on rich aggregates.
- Because shadows are lightweight DTOs, prefer them for UI/query use cases and reserve aggregates for invariants and write flows. You can still opt into causal consistency by wrapping reads with `QueryHandler` or `StartCausallyConsistentSectionAsync` when a client provides a token.

## Command handlers (write side)

`CommandHandler<T>` and `CommandHandler<T, TResult>` wrap MediatR handlers with transaction, retry, isolation-level, and causal-consistency helpers. Attributes like `RetryOnException`, `NoTransaction`, `WithIsolationLevel`, and `RequiresCausalConsistency` control the behavior.

This handler from Push Notification shows the typical pattern:

```csharp
using MongoDB.Bson;
using Pulsar.BuildingBlocks.DDD;
using Pulsar.BuildingBlocks.DDD.Attributes;
using Pulsar.BuildingBlocks.DDD.Contexts;
using Pulsar.BuildingBlocks.DDD.Abstractions;
using Pulsar.BuildingBlocks.Utils;
using Pulsar.Services.PushNotification.Contracts.Commands.Sessions;
using Pulsar.Services.PushNotification.Domain.Aggregates.Sessions;

namespace Pulsar.Services.PushNotification.API.Application.Commands.Sessions;

[RetryOnException(DuplicatedKey = true)]
public class CriarSessaoCH : CommandHandler<CriarSessaoCmd, CriarSessaoResult>
{
    private readonly ISessionRepository _sessionRepository;

    public CriarSessaoCH(
        ISessionRepository sessionRepository,
        IDbSession session,
        DbContextFactory contextFactory) : base(session, contextFactory)
    {
        _sessionRepository = sessionRepository;
    }

    protected override async Task<CriarSessaoResult> HandleAsync(CriarSessaoCmd cmd, CancellationToken ct)
    {
        var session = new Session(
            ObjectId.GenerateNewId(),
            cmd.UsuarioId.ToObjectId(),
            cmd.DominioId?.ToObjectId(),
            cmd.EstabelecimentoId?.ToObjectId(),
            GeneralExtensions.GetSalt(32));

        session.Criar();
        await _sessionRepository.InsertOneAsync(session, ct);
        return new CriarSessaoResult(session.Id.ToString(), session.Token);
    }
}
```

Key points:
- The base class opens a transaction by default and dispatches domain events after persistence.
- `RetryOnException(DuplicatedKey = true)` retries on duplicate-key or concurrency errors.
- Inject `IDbSession` and `DbContextFactory` so the base class can manage context and isolation.

### Service-specific handler pattern

Every microservice should expose its own base class on top of `CommandHandler` so **all** command handlers inside that service share the same dependency funnel (repositories, loggers, integration event log, etc.). Whether you name it `IdentityCommandHandler`, `FacilityCommandHandler`, or `PushNotificationCommandHandler`, the idea is identical. The Push Notification version looks like this:

```csharp
public abstract class PushNotificationCommandHandler<TRequest, TResponse>
    : CommandHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    protected ISessionRepository SessionRepository { get; }
    protected IShadowRepository<DominioShadow> DominioRepository { get; }
    protected ILogger Logger { get; }

    protected PushNotificationCommandHandler(
        PushNotificationCommandHandlerContext<TRequest, TResponse> ctx) : base(ctx.Session, ctx.DbContextFactory)
    {
        SessionRepository = (ISessionRepository)ctx.Repositories.First(r => r is ISessionRepository);
        DominioRepository = (IShadowRepository<DominioShadow>)ctx.Repositories.First(r => r is IShadowRepository<DominioShadow>);
        Logger = ctx.Logger;
    }
}
```

Pattern rationale (applies to every service):
- **Constructor hygiene**: each handler requests only the service-specific context so new dependencies get added in one spot.
- **Consistency**: transactions, logging, auditing, and integration-event persistence are configured once (the `CommandHandler` base plus the service context).
- **Testability**: handlers can be unit-tested by providing fake contexts with stub repositories instead of recreating the full DI graph.

When implementing your service-level base, keep the constructor lean, expose protected properties for the repositories every handler needs, and always pass the `IDbSession`/`DbContextFactory` up to `CommandHandler` so retry/transaction behaviors stay uniform across all microservices.

## Domain event handlers

Domain event handlers follow the same pattern. Each service defines its own base (for example, `PushNotificationDomainEventHandler<TEvent>`) that inherits from `DomainEventHandler<TEvent>` and accepts a context bundling the session, shared repositories, logger, and integration event log. Concrete handlers stay focused on the domain workflow:

```csharp
[RetryOnException(DuplicatedKey = true)]
public class CriarUserContextDEH : PushNotificationDomainEventHandler<SessaoCriadaDE>
{
    public CriarUserContextDEH(
        PushNotificationDomainEventHandlerContext<SessaoCriadaDE> ctx) : base(ctx)
    {
    }

    protected override async Task HandleAsync(SessaoCriadaDE evt, CancellationToken ct)
    {
        var spec = new FindUserContextSpec(evt.UsuarioId, evt.DominioId, evt.EstabelecimentoId);
        var userContext = await UserContextRepository.FindOneAsync(spec);
        if (userContext != null)
            return;

        await UserContextRepository.InsertOneAsync(
            new UserContext(ObjectId.GenerateNewId(), evt.UsuarioId, evt.DominioId, evt.EstabelecimentoId));
    }
}
```

Guidelines for service-specific domain event handlers (applies to every microservice):
- **Reuse the context** so every handler gets the same repositories, logger, and session scope without constructor bloat.
- **Stay side-effect focused**: the `DomainEventHandler` base ensures events run inside the same transaction (unless opted out) and honors retry/isolation attributes, so handler logic can concentrate on idempotent state changes.
- **Propagate cancellation tokens** when calling repositories to keep consistency with the ambient transaction lifetime.

Creating these thin derived classes keeps the entire solution uniform: the building blocks own transactional and consistency concerns, the service-level base injects shared infrastructure, and individual handlers only express the domain action triggered by a command or event.

## Domain vs integration events

Domain events (`INotification` instances raised through `AddDomainEvent`) never leave the bounded context. They are great for keeping aggregates small and letting in-process handlers, such as `PushNotificationDomainEventHandler<SessaoCriadaDE>`, react to state transitions without calling other microservices. Integration events, on the other hand, are serialized DTOs that cross boundaries through the event bus. They inherit from `Pulsar.BuildingBlocks.EventBus.Events.IntegrationEvent`, carry a durable `Id`/`CreationDate`, and live in the Contracts project so consumers in other services (or mobile apps) can reference them without pulling the entire domain.

Real integration events look like `Sources/Services/Identity/Identity.Contracts/IntegrationEvents/ConviteAceitoIE.cs`:

```csharp
[EventName("Identity:ConviteAceitoIE")]
[PushNotificationEvent(PushNotificationKey.ConviteAceito)]
public record ConviteAceitoIE : IntegrationEvent, IPushNotificationEvent
{
    public required string UsuarioId { get; init; }
    public required string UsuarioEmail { get; init; }
    public required string UsuarioConvidanteId { get; init; }
    public required string UsuarioConvidanteEmail { get; init; }
    public PushNotificationData? GetPushNotificationData() => ...;
}
```

### Saving integration events with `ISaveIntegrationEventLog`

`ISaveIntegrationEventLog` (`Sources/BuildingBlocks/EventBus/EventBus/Abstractions/ISaveIntegrationEventLog.cs`) gives handlers a single method, `SaveEventAsync`, to persist integration events inside the same Mongo transaction used for the aggregate. Every service-specific handler context (e.g., `PushNotificationCommandHandlerContext`, `IdentityCommandHandlerContext`) resolves this interface so commands and domain-event handlers can append events right after they mutate state:

```csharp
// Sources/Services/Identity/Identity.API/Application/Commands/Convites/AceitarConviteCH.cs
convite.Aceitar(...);
await ConviteRepository.ReplaceOneAsync(convite);
await EventLog.SaveEventAsync(new ConviteAceitoIE
{
    UsuarioConvidanteEmail = usuarioConvidante.Email ?? "Desconhecido",
    UsuarioConvidanteId = usuarioConvidante.Id.ToString(),
    UsuarioEmail = convite.Email,
    UsuarioId = convite.UsuarioId.ToString()
});
```

The default implementation, `MongoSaveIntegrationEventLog` (`Sources/BuildingBlocks/DDD/DDD.Mongo/EventBus/MongoSaveIntegrationEventLog.cs`), writes the serialized payload to the `IntegrationEventLog` collection using the ambient `MongoDbSession`. If the event implements `IPushNotificationEvent`, it also materializes a `PushNotificationIE` entry so mobile/web clients can receive the same intent without duplicating code:

```csharp
public async Task SaveEventAsync(IntegrationEvent @event, CancellationToken ct = default)
{
    var entry = new IntegrationEventLogEntry(@event);
    await _collection.InsertOneAsync(_session.CurrentHandle, entry, cancellationToken: ct);
    if (@event is IPushNotificationEvent pnEvent && pnEvent.GetPushNotificationData() is { } data)
    {
        var pnEntry = new IntegrationEventLogEntry(new PushNotificationIE(data));
        await _collection.InsertOneAsync(_session.CurrentHandle, pnEntry, cancellationToken: ct);
    }
}
```

Net effect: domain events keep invariants local, while integration events (plus their log entries) become the durable source for cross-service communication.

### Push notification payload contract

Any `IPushNotificationEvent` (`Sources/BuildingBlocks/EventBus/EventBus.Contracts/PushNotifications/IPushNotificationEvent.cs`) must return a fully populated `PushNotificationData` (`.../PushNotificationData.cs`) from `GetPushNotificationData()`. The object is what the push service saves and what the Blazor client (`Frontends/Web/Pulsar.Web/Pulsar.Web.Client/Services/PushNotifications/*`) renders, so every property has a defined responsibility.

#### Property reference

| Property | Type | Details |
| --- | --- | --- |
| `Target` | `PushNotificationTarget` (`.../PushNotificationTarget.cs`) | Identifies the recipient (`UsuarioId`, optional `DominioId`/`EstabelecimentoId`) plus the `Match` strategy (see `PushNotificationTargetMatch` below). Used by the push service to fan-out notifications and by clients to scope subscriptions. |
| `Key` | `PushNotificationKey` | Logical identifier that must match the `[PushNotificationEvent]` attribute attached to the integration event so consumers can resolve the correct handler. |
| `Title`, `Message` | `string?` | Primary copy for cards/toasts. `ToastDisplayOptions` decides whether the toast header shows the title, the message, or both, while notification centers commonly render both values. |
| `CreatedOn` | `DateTime` | Timestamp used for ordering and presentation. |
| `Intent` | `PushNotificationIntent` | Indicates which iconography/color scheme should represent the message (see table below). |
| `Display` | `PushNotificationDisplay` | Controls whether the client should emit a toast, a notification-center entry, both, or neither. |
| `ToastDisplayOptions` | `PushNotificationToastDisplayOptions` | Decides which string becomes the toast header/body when the toast channel is enabled. |
| `ToastActionOptions` | `PushNotificationToastActionOptions` | Picks which CTA (if any) becomes the toast action button. |
| `PrimaryAction`, `SecondaryAction`, `LabelAction` | `PushNotificationDataAction` (`.../PushNotificationDataAction.cs`) | Describe commands bound to buttons/links. Each action carries `Text`, a `RouteKey`, and optional `Parameters` (`PushNotificationDataActionParam`) interpreted by the routing layer. Multiple actions can coexist; `ToastActionOptions` determines which ones surface in toasts. |
| `Data` | `string?` (JSON) | Optional payload that backs strongly typed events so subscribers can deserialize domain-specific context (e.g., the `ConviteAceitoIE` data contract). |

Filling these properties is what lets every client reuse a single rendering pipeline: the Identity `ConviteAceitoIE` example above sets `Message`, `Intent`, `Display`, and `Target`, and the consumer automatically decides how to present it.

#### Enum reference

##### `PushNotificationTargetMatch` (`.../PushNotificationTargetMatch.cs`)
The filters used by each option are implemented in `FindUserContextsByTargetSpec` (`Services/PushNotification/PushNotification.Domain/Specifications/UserContexts/FindUserContextsByTargetSpec.cs`). That specification translates the enum into Mongo predicates over the `UserContext` collection, so the exact behavior below mirrors the real query paths:
- `ExactMatch`: requires either `Target.DominioId` or `Target.EstabelecimentoId` (otherwise the spec returns an empty filter). When `DominioId` is provided, it matches `UsuarioId`, `DominioId`, and `EstabelecimentoId` exactly—even if the establishment is `null`, which targets the domain root. When only an establishment is provided, it matches `UsuarioId` + `EstabelecimentoId`. Example: `DominioId=D1`, `EstabelecimentoId=null` notifies U1 only when they are in D1's domain dashboard; `DominioId=D1`, `EstabelecimentoId=E1` reaches the single clinic context instead.
- `MatchUsuarioOnly`: filters solely by `UsuarioId`, ignoring domain and establishment altogether. Example: send a password expiry alert to every session opened by U1 regardless of where they are browsing.
- `MatchUsuarioDominio`: filters by `UsuarioId`, `DominioId`, and enforces `EstabelecimentoId == null`. Use it for domain-level announcements that should only appear when the user is on the root domain view, not drilled into any establishment. Example: notify U1 about a new governance policy while they are at D1's overview page.
- `MatchUsuarioEstabelecimentos`: filters by `UsuarioId` and requires `EstabelecimentoId != null`, leaving `DominioId` unconstrained. This hits every establishment session the user currently owns, even across different domains. Example: warn U1 about expiring room licenses in whichever clinics they are inside.
- `MatchUsuarioEstabelecimentosFromDominio`: filters by `UsuarioId`, `DominioId`, and requires `EstabelecimentoId != null`. That combination fans out to every establishment session under the specified domain, but nowhere else. Example: broadcast a rollout notice to all of U1's clinics within `DominioId=D1` without touching clinics the user operates in other domains.
- `MatchTodosUsuariosDominio`: ignores `UsuarioId` and targets every session in the specified `DominioId` where `EstabelecimentoId == null` (the domain root). Example: announce a maintenance window to every administrator currently scoped to D1's overview page.
- `MatchTodosUsuariosEstabelecimento`: ignores `UsuarioId` and targets everyone whose `EstabelecimentoId` matches `Target.EstabelecimentoId`, regardless of domain shadow state. Example: alert all staff inside clinic E1 about a temporary outage, no matter which user initiated the event.
- `MatchTodosUsuariosEmEstabelecimentosDoDominio`: ignores `UsuarioId`, requires `DominioId`, and matches every session under that domain where `EstabelecimentoId != null`. Example: broadcast a vaccine recall to every establishment session inside D1 while leaving domain-root sessions untouched.

##### `PushNotificationDisplay` (`.../PushNotificationDisplay.cs`)
- `None`: suppress both toast and notification-center rendering (useful when an event only refreshes badges).
- `Toast`: only raise the transient toast via `ToastService`.
- `NotificationCenter`: only persist it in the Fluent UI message list.
- `All`: do both (default behavior across the solution).

##### `PushNotificationToastDisplayOptions` (`.../PushNotificationToastOptions.cs`)
- `UseTitle`: toast title shows `Title`, body stays empty.
- `UseMessage`: toast title shows `Message`, mirroring “message-only” banners.
- `UseBoth`: use the Fluent UI communication toast component to show both title and message (SignalR manager picks this automatically when `ToastActionOptions` is `UsePrimaryAndSecondaryAction`).

##### `PushNotificationToastActionOptions` (`.../PushNotificationToastOptions.cs`)
- `NoAction`: toast is informational only.
- `UseLabel`: single action button bound to `LabelAction`.
- `UsePrimaryAction`: button bound to `PrimaryAction` (typically the main CTA).
- `UseSecondaryAction`: button bound to `SecondaryAction`.
- `UsePrimaryAndSecondaryAction`: renders both primary and secondary buttons (forces `ToastDisplayOptions.UseBoth` so there is enough layout space).

##### `PushNotificationIntent` (`.../PushNotificationIntent.cs`)
Each value maps to a different intent/icon so clients can pick consistent visuals:
- `None`: neutral message with no icon.
- `Error`, `Warning`, `Information`, `Success`: map directly to Fluent UI `MessageIntent`/`ToastIntent` to render red/amber/blue/green system banners.
- `Flash`: progress/lightning icon for urgent or in-progress items.
- `Calendar`: calendar glyph for scheduling updates.
- `Upload` / `Download`: arrow icons for transfer operations.
- `Person`: mention badge for user-focused events.
- `Alert`: neutral alert icon (used for general warnings).
- `Delete`: trash icon for destructive outcomes.
- `News`: newspaper icon for announcements.
- `Edit`: pencil icon for editable content.
- `New`: badge icon for brand-new objects.
- `Add`: plus icon for creation flows.
- `Heart`: heart icon for social/relationship cues.
- `Sync`: sync arrows for background jobs.
- `Save`: floppy icon for persistence events.
- `Folder`: folder icon for file/workspace notifications.
- `Star`: star icon for featured/favorite content.
- `Mail`: envelope icon for messaging.
- `Home`: home icon for tenancy/organization updates.

##### `PushNotificationTarget`, `PushNotificationDataAction`, and routing keys
- `PushNotificationTarget` holds `UsuarioId` plus optional `DominioId` and `EstabelecimentoId`. Its `Clone()` method allows the push service to safely duplicate targets when batching.
- `PushNotificationDataAction` wraps CTA metadata; it clones parameters so `PushNotificationManager` can reuse cached payloads without mutating shared state.
- `PushNotificationRouteKey` and `PushNotificationRouteParamKey` live in `Sources/Services/Shared/Shared/PushNotifications/` and are typically extended per frontend to describe navigation routes and template parameters consumed by `PushNotificationRoutingAttribute`.

### Registering push notification event types

Every class that implements `IPushNotificationEvent` must also be annotated with `[PushNotificationEvent(...)]` (`Sources/Services/Shared/Shared/PushNotifications/PushNotificationEventAttribute.cs`). The Blazor `SignalRManager` (`Frontends/Web/Pulsar.Web/Pulsar.Web.Client/Services/PushNotifications/SignalRManager.cs`) scans all loaded assemblies for that attribute, builds a dictionary that maps each `PushNotificationKey` to the concrete event type, and then uses `PushNotificationEvent.StronglyTyped` plus MediatR to fan out strongly typed notifications (`SignalRManager.FireAdditionalEvents`). Without the attribute the event is invisible to the scanner, meaning:

1. Subscribers that call `SignalRManager.Subscribe(key, handler)` will never fire because the key is not registered.
2. `PushNotificationEvent<TData>` will never be instantiated, so components expecting a typed payload cannot deserialize the `Data` blob.

Attaching the attribute is therefore mandatory for every `IPushNotificationEvent`; it ties the integration event to a `PushNotificationKey`, gives the UI enough metadata to deserialize `Data`, and keeps the SignalR dispatcher aligned with the actions declared in `PushNotificationData`.

### Dispatching the log to the bus

`IntegrationEventLogMongo` stores events alongside a small leasing system (`EventProducer` collection) so multiple dispatcher instances can partition the work. `GenericIntegrationEventDispatcherService` (`Sources/BuildingBlocks/EventBus/EventBus/GenericIntegrationEventDispatcherService*.cs`) hosts two loops:

1. A **producer** watches and polls `IntegrationEventLogEntry` documents via `MongoIntegrationEventLogStorage` (`IntegrationEventLogMongo/MongoIntegrationEventLogStorage.cs`), batching the records that are due.
2. Several **consumers** pop those batches, mark them as `InProgress`, publish each payload through the configured `IEventBus`, and update the log to `Published`, `Pending` (rescheduled), or `Failed` depending on the outcome.

Because the dispatcher uses the same log abstraction everywhere (`IIntegrationEventLogStorage`), you can run it in API hosts, workers, or Azure Functions without special plumbing—the only responsibility of the command/domain-event handler is to call `EventLog.SaveEventAsync`. Once the handler transaction commits, the dispatcher eventually pushes the event to Azure Service Bus (or any other `IEventBus` implementation) with retries, exponential backoff, and automatic rescheduling if a consumer is temporarily down.

## Domain exceptions

`DomainException` is the shared base for business-rule violations. Each microservice must derive its own strongly typed exception (e.g., `IdentityDomainException`, `CatalogDomainException`, `PushNotificationDomainException`) so:

- Keys live in the service’s contracts and can be surfaced to clients or other services without exposing internal strings.
- Middleware (like `Shared.Web`’s `JsonExceptionMiddleware`) can map a service’s exceptions to consistent HTTP payloads.
- Unit tests can assert against service-specific exception types instead of generic messages.

Real example from the identity service (`Sources/Services/Identity/Identity.Domain/Exceptions/IdentityDomainException.cs`):

```csharp
public class IdentityDomainException : DomainException
{
    public IdentityExceptionKey Key { get; }

    public IdentityDomainException(IdentityExceptionKey key) : this(key, GetMessageFromKey(key))
    {
    }

    public IdentityDomainException(IdentityExceptionKey key, string message)
        : base(key.ToString(), message)
    {
        Key = key;
    }

    public IdentityDomainException(IdentityExceptionKey key, string message, Exception innerException)
        : base(key.ToString(), message, innerException)
    {
        Key = key;
    }
}
```

The keys themselves live in the service contracts, e.g. `Sources/Services/Identity/Identity.Contracts/Enumerations/IdentityExceptionKey.cs`:

```csharp
public enum IdentityExceptionKey
{
    [Display(Description = "Usuário não encontrado.")]
    UsuarioNaoEncontrado = 1,
    [Display(Description = "Usuário não possui e-mail cadastrado.")]
    UsuarioSemEmail = 2,
    [Display(Description = "Token para a mudança de senha expirado.")]
    TokenMudancaSenhaExpirado = 3,
    [Display(Description = "Token para a mudança de senha inválido.")]
    TokenMudancaSenhaInvalido = 4,
    [Display(Description = "Convite não encontrado.")]
    ConviteNaoEncontrado = 5,
    [Display(Description = "Convite expirado.")]
    ConviteExpirado = 6,
    [Display(Description = "Este convite já foi aceito.")]
    ConviteJaAceito = 7,
    [Display(Description = "Já existe um usuário para o e-mail informado neste convite.")]
    UsuarioJaConvidado = 8,
    [Display(Description = "Token inválido.")]
    ConviteTokenInvalido = 9,
    [Display(Description = "Usuário foi convidado para administrar o domínio mas o domínio já tem administrador ou está inativo.")]
    ConviteDominioInvalido = 10,
    [Display(Description = "Convite Inválido.")]
    ConviteInvalido = 11,
    [Display(Description = "Já existe um usuário com o nome de usuário informado.")]
    NomeUsuarioNaoUnico = 12,
    [Display(Description = "Convite para este usuário ainda não foi aceito.")]
    ConviteNaoAceito = 13,
    [Display(Description = "Senha atual inválida.")]
    SenhaAtualInvalida = 14,
    [Display(Description = "O usuário 'administrador' não pode ser bloqueado/desbloqueado.")]
    SuperUsuarioNaoPodeSerBloqueado = 15,
    [Display(Description = "O usuário 'administrador' não pode administrar um domínio.")]
    SuperUsuarioNaoPodeAdministrarDominio = 16,
    [Display(Description = "Domínio não encontrado.")]
    DominioNaoEncontrado = 17,
    [Display(Description = "O usuário informado para administrar este domínio está bloqueado nele.")]
    UsuarioAdministradorIsBloqueadoDominio = 18,
    [Display(Description = "O usuário administrador não pode ser bloqueado neste domínio.")]
    UsuarioAdministradorNaoPodeSerBloqueadoDominio = 19,
    [Display(Description = "O usuário 'administrador' não pode ser bloqueado ou desbloqueado dentro deste domínio.")]
    SuperUsuarioNaoPodeSerBloqueadoDominio = 20,
    [Display(Description = "Grupo não encontrado.")]
    GrupoNaoEncontrado = 21,
    [Display(Description = "Subgrupo com o nome informado já existe.")]
    SubgrupoJaExistente = 22,
    [Display(Description = "Subgrupo não encontrado.")]
    SubgrupoNaoEncontrado = 23,
    [Display(Description = "O usuário 'administrador' não pode ser adicionado ou removido de um grupo.")]
    SuperUsuarioNaoPodeserAdicionadoEmGrupo = 24,
    [Display(Description = "Você não está logado em um domínio.")]
    DominioNaoLogado = 25,
    [Display(Description = "Estabelecimento não encontrado.")]
    EstabelecimentoNaoEncontrado = 26,
    [Display(Description = "Rede de Estabelecimentos não encontrado.")]
    RedeEstabelecimentosNaoEncontrado = 27,
    [Display(Description = "Você não está logado em um estabelecimento.")]
    EstabelecimentoNaoLogado = 28,
    [Display(Description = "Um grupo pode ter no máximo 100 subgrupos.")]
    NumSubgruposExcedeMaximo = 29,
    [Display(Description = "Um grupo pode ter no máximo 5000 usuários membros.")]
    NumUsuariosGrupoExcedeMaximo = 30,
}
```

Usage pattern:
- Define an enum (e.g., `IdentityExceptionKey`) describing every domain error with a stable key.
- Derive `YourServiceDomainException : DomainException` and capture the enum value in the constructor.
- Throw the service-specific exception from aggregates/handlers (`throw new IdentityDomainException(IdentityExceptionKey.UsuarioNaoEncontrado);`).
- Api/worker middleware logs the key and translates it to error responses while tests assert `await Assert.ThrowsAsync<IdentityDomainException>(...)`.

Following this pattern makes cross-service error handling predictable and avoids stringly-typed exception logic.

## DbContext and AggregateRootWithContext

`DbContext` is the ambient gateway that gives aggregates read access to other aggregates within the same transaction. Each handler receives a `DbContextFactory` (see `DDD/Contexts/DbContextFactory.cs`) built from the resolved repositories. The base `CommandHandler`/`DomainEventHandler` creates a `DbContextImpl`, pushes it via `DbContextImpl.SetContext`, and clears it after execution, so `DDD.Contracts.DbContext.Current` always points to a live `IDbContext` during handler execution.

`AggregateRootWithContext<T>` extends `AggregateRoot` and exposes navigation-style helpers backed by that ambient context (file `DDD.Contracts/AggregateRootWithContext.cs`):

```csharp
public abstract class AggregateRootWithContext<TSelf> : AggregateRoot
        where TSelf : class, IAggregateRoot
{
        public static Task<bool> Exists(ObjectId id) => DbContext.Current.Exists<TSelf>(id);
        public static Task<TSelf?> TryGet(ObjectId id) => DbContext.Current.TryGet<TSelf>(id);
        public static Task<TSelf> Get(ObjectId id) => DbContext.Current.Get<TSelf>(id);
        public static Task<TSelf> GetAndCache(ObjectId id, string key) => DbContext.Current.GetAndCache<TSelf>(id, key);
        // ... additional helpers (TryGetAndCache, GetMany, etc.)
}
```

Real usage examples:
- `Convite` exposes a navigation helper that returns the invited `Usuario` by calling those static helpers (file `Services/Identity/Identity.Domain/Aggregates/Convites/Convite.cs`):

    ```csharp
    public Task<Usuario> GetUsuario() => Usuario.GetAndCache(this.UsuarioId, nameof(UsuarioId));
    ```

    Domain logic can invoke `await convite.GetUsuario()` anywhere inside the aggregate without receiving an `IUsuarioRepository`; the ambient `DbContext` resolves the repository, issues the query with the same Mongo session/transaction, and caches the result by key for the lifetime of the aggregate instance.

- All shadows inherit from `Shadow<T>` which itself derives from `AggregateRootWithContext<T>` (`BuildingBlocks/Sync/Sync.Contracts/Shadow.cs`). That gives projection types, such as `Services/Facility/Facility.Contracts/Shadows/EstabelecimentoShadow`, the same ability to pull additional aggregates when the synchronization pipeline materializes or enriches shadows.

Guidelines:
- Use the helpers for read-only lookups that support your invariant checks or domain events; writes still go through repositories so concurrency rules stay centralized.
- Prefer `GetAndCache(id, key)` if you expect repeated reads of the same aggregate inside one operation; the key scopes a per-aggregate cache so you avoid duplicate queries.
- Because the context runs inside the handler’s session, all reads observe the same snapshot as the write model, mirroring ORM-style navigation properties but without leaking repository dependencies into domain code.

## Version concurrency

`VersionConcurrencyException` shields aggregates from lost updates. Every document persisted through `MongoRepository` carries a `Version` column that increments on each `ReplaceOne`/`Update`. Repository methods filter by the previous version and call `CheckModified()` to ensure exactly one document was touched; if MongoDB reports zero modifications, a `VersionConcurrencyException` is thrown.

Patterns for application code:
- **Always read the aggregate before writing** so `LastVersion` is populated.
- **Leave `checkModified` enabled** (default) so repository methods throw when the version does not match.
- **Wrap command handlers with `[RetryOnException(VersionConcurrency = true)]`** when concurrent updates are expected. The building-block handler catches `VersionConcurrencyException` and retries the command a configurable number of times (see `RetryOnExceptionAttribute`).
- **Use `CheckModified()` after custom update specs** (`await repo.UpdateManyAsync(spec).CheckModified();`) to surface concurrency conflicts even when working with specifications instead of aggregates.

Real handler usage (Push Notification service):

```csharp
[NoTransaction, RetryOnException(VersionConcurrency = true)]
public class MarcarNotificacoesComoLidaCH : PushNotificationCommandHandler<MarcarNotificacoesComoLidaCmd, CommandResult>
{
    protected override async Task<CommandResult> HandleAsync(MarcarNotificacoesComoLidaCmd cmd, CancellationToken ct)
    {
        var spec = new MarcarNotificacoesComoLidaSpec(cmd.Notificacoes.Select(n => n.ToObjectId()).ToList(), cmd.UsuarioId!.ToObjectId());
        await NotificacaoPushRepository.UpdateManyAsync(spec).CheckModified();
        return new CommandResult();
    }
}
```

Identity commands follow the same pattern (`[RetryOnException(VersionConcurrency = true, Retries = 2)]`) so temporary conflicts just re-read the aggregate and try again. If retries are exhausted, the exception bubbles up and gets serialized by the shared middleware, prompting the caller to refresh state.

## Queries

`DDD.Mongo` includes a lightweight `QueryHandler` for read models that need causal consistency. Register it by scanning assemblies in `AddMongoDB` and inject `MongoDbSessionFactory`/`QueryHandler` into your query services. Use `StartCausallyConsistentSectionAsync` when a client passes a consistency token.

## Validation

`AddValidators` discovers FluentValidation validators in the provided assemblies. `ValidatorBehavior` runs them automatically in the MediatR pipeline, so validation failures surface before your handler executes.

## Transactions, isolation, and consistency

- Default behavior: each command runs inside a MongoDB transaction with write concern/read concern set to majority.
- Use `[WithIsolationLevel(IsolationLevel.Snapshot)]` to override the isolation level for a handler.
- Use `[NoTransaction]` to run without a transaction.
- Use `[RequiresCausalConsistency]` to start a causally consistent section when a consistency token is provided.
- `RetryOnException` supports retrying on duplicate keys, version concurrency, or custom exception types.

## Tips

- Always expose repository interfaces from the domain project; only infrastructure depends on MongoDB.
- Keep queries in specs so they are reusable in handlers and tests.
- If you need to run outside the current transaction/session (e.g., background read), call `EscapeSession()` on a repository.
- Domain events are raised inside aggregates; use `DomainEventHandler<TEvent>` classes in your API/worker to react and publish integration events if needed.
