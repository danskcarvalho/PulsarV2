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
