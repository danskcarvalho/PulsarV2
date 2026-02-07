# Domain-Driven Design (DDD) Building Blocks

This guide provides comprehensive documentation for using the Pulsar V2 DDD building blocks in your microservices. These libraries provide a solid foundation for implementing Domain-Driven Design patterns with MongoDB persistence.

## Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Getting Started](#getting-started)
- [Core Concepts](#core-concepts)
  - [Aggregate Roots](#aggregate-roots)
  - [Value Objects](#value-objects)
  - [Aggregate Components](#aggregate-components)
  - [Repositories](#repositories)
  - [Specifications](#specifications)
  - [Command Handlers](#command-handlers)
  - [Domain Event Handlers](#domain-event-handlers)
  - [Query Handlers](#query-handlers)
- [Advanced Features](#advanced-features)
  - [Transactions and Isolation Levels](#transactions-and-isolation-levels)
  - [Retry Policies](#retry-policies)
  - [Causal Consistency](#causal-consistency)
  - [Pagination and Cursors](#pagination-and-cursors)
  - [Validation](#validation)
  - [Audit Information](#audit-information)
- [Best Practices](#best-practices)

---

## Overview

The DDD building blocks consist of three main libraries:

1. **DDD.Contracts** - Core abstractions and base classes (domain layer)
2. **DDD** - Implementation of command handlers, domain events, and infrastructure abstractions
3. **DDD.Mongo** - MongoDB-specific implementations and behaviors

These libraries work together to provide:
- ? Aggregate Root and Entity patterns
- ? Value Object support
- ? Repository pattern with MongoDB
- ? CQRS with MediatR integration
- ? Domain Events
- ? Specification pattern
- ? Transaction management
- ? Optimistic concurrency control
- ? Automatic validation with FluentValidation
- ? Audit tracking

---

## Architecture

```
???????????????????????????????????????????????????????????????
?                     Your Application                         ?
?  ????????????????  ????????????????  ????????????????      ?
?  ?   Commands   ?  ?   Queries    ?  ?    Domain    ?      ?
?  ?   Handlers   ?  ?   Handlers   ?  ?    Events    ?      ?
?  ????????????????  ????????????????  ????????????????      ?
???????????????????????????????????????????????????????????????
                            ?
???????????????????????????????????????????????????????????????
?                    DDD Building Blocks                       ?
?  ???????????????????????????????????????????????????????   ?
?  ?  CommandHandler / DomainEventHandler / QueryHandler ?   ?
?  ???????????????????????????????????????????????????????   ?
?  ???????????????????????????????????????????????????????   ?
?  ?         Repository Pattern & Specifications          ?   ?
?  ???????????????????????????????????????????????????????   ?
?  ???????????????????????????????????????????????????????   ?
?  ?          AggregateRoot / ValueObject                 ?   ?
?  ???????????????????????????????????????????????????????   ?
???????????????????????????????????????????????????????????????
                            ?
???????????????????????????????????????????????????????????????
?                     MongoDB Driver                           ?
???????????????????????????????????????????????????????????????
```

---

## Getting Started

### 1. Install NuGet Packages

Add these packages to your projects:

**Domain Project:**
```xml
<PackageReference Include="DDD.Contracts" />
```

**API/Infrastructure Project:**
```xml
<PackageReference Include="DDD" />
<PackageReference Include="DDD.Mongo" />
```

### 2. Configure Services

In your `Program.cs` or startup configuration:

```csharp
using Pulsar.BuildingBlocks.DDD.Mongo;

var builder = WebApplication.CreateBuilder(args);

// Add DDD with MongoDB
builder.Services.AddDDDMongo(
    mongoDbConnectionString: builder.Configuration.GetConnectionString("MongoDb")!,
    mongoDbName: "YourDatabaseName",
    configureOptions: options =>
    {
        // Optional: Configure additional settings
        options.DefaultTransactionTimeout = TimeSpan.FromSeconds(30);
    }
);

// Register your repositories
builder.Services.AddScoped<IUsuarioRepository, UsuarioMongoRepository>();
builder.Services.AddScoped<IDominioRepository, DominioMongoRepository>();

// Add MediatR for CQRS
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssemblyContaining<Program>());

// Add FluentValidation for automatic validation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
```

### 3. Create Your First Aggregate

```csharp
using DDD.Contracts;

namespace YourService.Domain.Aggregates.Users;

public class User : AggregateRoot
{
    private string _firstName;
    private string _lastName;
    
    // Constructor for new aggregates
    public User(string firstName, string lastName, string email)
    {
        Id = ObjectId.GenerateNewId();
        _firstName = firstName;
        _lastName = lastName;
        Email = email;
        IsActive = true;
        AuditInfo = AuditInfo.Create();
        
        // Raise domain event
        AddDomainEvent(new UserCreatedDE(Id, email));
    }
    
    // Constructor for MongoDB deserialization
    [BsonConstructor]
    public User(ObjectId id, string firstName, string lastName, string email, 
                bool isActive, AuditInfo auditInfo) : base(id)
    {
        _firstName = firstName;
        _lastName = lastName;
        Email = email;
        IsActive = isActive;
        AuditInfo = auditInfo;
    }
    
    public string FirstName
    {
        get => _firstName;
        set
        {
            _firstName = value;
            UpdateSearchTerms();
        }
    }
    
    public string LastName
    {
        get => _lastName;
        set
        {
            _lastName = value;
            UpdateSearchTerms();
        }
    }
    
    public string Email { get; private set; }
    public bool IsActive { get; set; }
    public AuditInfo AuditInfo { get; set; }
    public string SearchTerms { get; private set; }
    
    private void UpdateSearchTerms()
    {
        SearchTerms = $"{_firstName};{_lastName};{Email}".ToLowerInvariant();
    }
    
    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException("User.AlreadyDeactivated");
            
        IsActive = false;
        AddDomainEvent(new UserDeactivatedDE(Id));
    }
}
```

---

## Core Concepts

### Aggregate Roots

Aggregate Roots are the main entities in your domain that maintain consistency boundaries.

**Key Features:**
- Unique identity (ObjectId)
- Version tracking for optimistic concurrency
- Domain event collection
- Lifecycle management

**Base Classes:**

1. **`AggregateRoot`** - Standard aggregate root
2. **`AggregateRootWithContext<T>`** - Aggregate root with typed context access

**Example:**

```csharp
public class Order : AggregateRoot
{
    public Order(ObjectId customerId, List<OrderItem> items)
    {
        Id = ObjectId.GenerateNewId();
        CustomerId = customerId;
        Items = items;
        Status = OrderStatus.Pending;
        TotalAmount = items.Sum(i => i.Price * i.Quantity);
        CreatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new OrderCreatedDE(Id, customerId, TotalAmount));
    }
    
    [BsonConstructor]
    public Order(ObjectId id, ObjectId customerId, List<OrderItem> items,
                 OrderStatus status, decimal totalAmount, DateTime createdAt) 
        : base(id)
    {
        CustomerId = customerId;
        Items = items;
        Status = status;
        TotalAmount = totalAmount;
        CreatedAt = createdAt;
    }
    
    public ObjectId CustomerId { get; private set; }
    public List<OrderItem> Items { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    
    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException("Order.InvalidStatus");
            
        Status = OrderStatus.Confirmed;
        AddDomainEvent(new OrderConfirmedDE(Id));
    }
}
```

### Value Objects

Value Objects are immutable objects defined by their attributes rather than identity.

```csharp
using Pulsar.BuildingBlocks.DDD;

public class Address : ValueObject
{
    public Address(string street, string city, string state, string zipCode)
    {
        Street = street;
        City = city;
        State = state;
        ZipCode = zipCode;
    }
    
    public string Street { get; }
    public string City { get; }
    public string State { get; }
    public string ZipCode { get; }
    
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return State;
        yield return ZipCode;
    }
}

public class Money : ValueObject
{
    public Money(decimal amount, string currency)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative");
            
        Amount = amount;
        Currency = currency;
    }
    
    public decimal Amount { get; }
    public string Currency { get; }
    
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
    
    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot add money with different currencies");
            
        return new Money(Amount + other.Amount, Currency);
    }
}
```

### Aggregate Components

Aggregate Components are parts of an aggregate that need lifecycle tracking.

```csharp
using Pulsar.BuildingBlocks.DDD;

public class OrderItem : AggregateComponent
{
    public OrderItem(ObjectId productId, string productName, int quantity, decimal price)
    {
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        Price = price;
    }
    
    public ObjectId ProductId { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    
    public decimal GetTotal() => Quantity * Price;
}
```

### Repositories

Repositories provide data access abstraction for aggregates.

**Define Repository Interface:**

```csharp
namespace YourService.Domain.Aggregates.Users;

public interface IUserRepository : IRepository<IUserRepository, User>
{
    // Add custom methods if needed
}
```

**Implement Repository:**

```csharp
using Pulsar.BuildingBlocks.DDD.Mongo.Implementations;

namespace YourService.Infrastructure.Repositories;

public class UserMongoRepository : MongoRepository<IUserRepository, User>, 
                                    IUserRepository
{
    public UserMongoRepository(MongoDbSession? session, 
                               MongoDbSessionFactory sessionFactory) 
        : base(session, sessionFactory)
    {
    }

    protected override string CollectionName => "users";

    protected override IUserRepository Clone(MongoDbSession? session, 
                                             MongoDbSessionFactory sessionFactory)
    {
        return new UserMongoRepository(session, sessionFactory);
    }
}
```

**Using Repositories:**

```csharp
// Find by ID
var user = await userRepository.FindOneByIdAsync(userId);

// Find with specification
var spec = new FindActiveUsersSpec();
var activeUsers = await userRepository.FindManyAsync(spec);

// Insert
await userRepository.InsertOneAsync(newUser);

// Update (with optimistic concurrency check)
user.UpdateEmail("new@email.com");
await userRepository.ReplaceOneAsync(user, checkModified: true);

// Delete
await userRepository.DeleteOneByIdAsync(userId);
```

### Specifications

Specifications encapsulate query logic in a reusable way.

**Find Specification:**

```csharp
using Pulsar.BuildingBlocks.DDD.Abstractions;

public class FindActiveUsersSpec : IFindSpecification<User>
{
    public FindSpecification<User> GetSpec()
    {
        return new FindSpecification<User>(
            predicate: u => u.IsActive == true,
            orderBy: new[] { Ordering<User>.Asc(u => u.LastName) },
            skip: null,
            limit: 100
        );
    }
}

public class FindUserByEmailSpec : IFindSpecification<User>
{
    private readonly string _email;
    
    public FindUserByEmailSpec(string email)
    {
        _email = email.ToLowerInvariant();
    }
    
    public FindSpecification<User> GetSpec()
    {
        return new FindSpecification<User>(
            predicate: u => u.Email == _email,
            orderBy: null,
            skip: null,
            limit: 1
        );
    }
}
```

**Find Specification with Projection:**

```csharp
public class UserSummary
{
    public ObjectId Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
}

public class FindUserSummariesSpec : IFindSpecification<User, UserSummary>
{
    public FindSpecification<User, UserSummary> GetSpec()
    {
        return new FindSpecification<User, UserSummary>(
            predicate: u => u.IsActive,
            projection: u => new UserSummary
            {
                Id = u.Id,
                FullName = u.FirstName + " " + u.LastName,
                Email = u.Email
            },
            orderBy: new[] { Ordering<User>.Asc(u => u.LastName) },
            skip: null,
            limit: null
        );
    }
}

// Usage
var summaries = await userRepository.FindManyAsync(new FindUserSummariesSpec());
```

**Update Specification:**

```csharp
public class DeactivateUsersInDomainSpec : IUpdateSpecification<User>
{
    private readonly ObjectId _domainId;
    
    public DeactivateUsersInDomainSpec(ObjectId domainId)
    {
        _domainId = domainId;
    }
    
    public UpdateSpecification<User> GetSpec()
    {
        return new UpdateSpecification<User>(
            predicate: u => u.DomainId == _domainId && u.IsActive,
            update: Builders<User>.Update
                .Set(u => u.IsActive, false)
                .Set(u => u.DeactivatedAt, DateTime.UtcNow)
        );
    }
}

// Usage
await userRepository.UpdateManyAsync(new DeactivateUsersInDomainSpec(domainId));
```

**Delete Specification:**

```csharp
public class DeleteInactiveUsersSpec : IDeleteSpecification<User>
{
    private readonly DateTime _beforeDate;
    
    public DeleteInactiveUsersSpec(DateTime beforeDate)
    {
        _beforeDate = beforeDate;
    }
    
    public DeleteSpecification<User> GetSpec()
    {
        return new DeleteSpecification<User>(
            predicate: u => !u.IsActive && u.DeactivatedAt < _beforeDate
        );
    }
}
```

### Command Handlers

Command Handlers process commands in a CQRS architecture.

**Create Command:**

```csharp
namespace YourService.Contracts.Commands.Users;

public record CreateUserCmd : IRequest<string>
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
}
```

**Create Validator:**

```csharp
using FluentValidation;

public class CreateUserCmdValidator : AbstractValidator<CreateUserCmd>
{
    public CreateUserCmdValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100);
            
        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100);
            
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
    }
}
```

**Implement Command Handler:**

```csharp
using Pulsar.BuildingBlocks.DDD;
using Pulsar.BuildingBlocks.DDD.Attributes;

namespace YourService.API.Application.Commands.Users;

// Automatically retries on duplicate key errors
[RetryOnException(DuplicatedKey = true)]
public class CreateUserCH : CommandHandler<CreateUserCmd, string>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<CreateUserCH> _logger;
    
    public CreateUserCH(
        IDbSession session, 
        DbContextFactory contextFactory,
        IUserRepository userRepository,
        ILogger<CreateUserCH> logger) 
        : base(session, contextFactory)
    {
        _userRepository = userRepository;
        _logger = logger;
    }
    
    protected override async Task<string> HandleAsync(CreateUserCmd cmd, 
                                                       CancellationToken ct)
    {
        // Check if user already exists
        var existingUser = await _userRepository.FindOneAsync(
            new FindUserByEmailSpec(cmd.Email), ct);
            
        if (existingUser != null)
            throw new DomainException("User.EmailAlreadyExists");
        
        // Create new user
        var user = new User(cmd.FirstName, cmd.LastName, cmd.Email);
        
        // Insert into database
        await _userRepository.InsertOneAsync(user, ct);
        
        _logger.LogInformation("Created user {UserId} with email {Email}", 
            user.Id, user.Email);
        
        return user.Id.ToString();
    }
}
```

**Using Base Command Handler:**

For consistency across your service, create a base command handler:

```csharp
public abstract class YourServiceCommandHandler<TRequest> 
    : CommandHandler<TRequest> where TRequest : IRequest
{
    protected IUserRepository UserRepository { get; }
    protected ILogger Logger { get; }
    
    protected YourServiceCommandHandler(
        YourServiceCommandHandlerContext<TRequest> ctx) 
        : base(ctx.Session, ctx.DbContextFactory)
    {
        UserRepository = ctx.UserRepository;
        Logger = ctx.Logger;
    }
}

// Usage
public class UpdateUserCH : YourServiceCommandHandler<UpdateUserCmd>
{
    public UpdateUserCH(YourServiceCommandHandlerContext<UpdateUserCmd> ctx) 
        : base(ctx)
    {
    }
    
    protected override async Task HandleAsync(UpdateUserCmd cmd, CancellationToken ct)
    {
        var user = await UserRepository.FindOneByIdAsync(cmd.UserId.ToObjectId(), ct);
        // ... update logic
    }
}
```

### Domain Event Handlers

Domain Event Handlers respond to domain events.

**Define Domain Event:**

```csharp
using MediatR;

namespace YourService.Domain.Events.Users;

public record UserCreatedDE : INotification
{
    public ObjectId UserId { get; init; }
    public string Email { get; init; }
    
    public UserCreatedDE(ObjectId userId, string email)
    {
        UserId = userId;
        Email = email;
    }
}
```

**Implement Event Handler:**

```csharp
using Pulsar.BuildingBlocks.DDD;

public class UserCreatedDEH : DomainEventHandler<UserCreatedDE>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<UserCreatedDEH> _logger;
    
    public UserCreatedDEH(
        IDbSession session,
        DbContextFactory contextFactory,
        IEmailService emailService,
        ILogger<UserCreatedDEH> logger)
        : base(session, contextFactory)
    {
        _emailService = emailService;
        _logger = logger;
    }
    
    protected override async Task HandleAsync(UserCreatedDE evt, CancellationToken ct)
    {
        _logger.LogInformation("Sending welcome email to {Email}", evt.Email);
        
        await _emailService.SendWelcomeEmailAsync(evt.Email, ct);
    }
}
```

**Multiple Event Handlers:**

You can have multiple handlers for the same event:

```csharp
public class UserCreatedNotificationDEH : DomainEventHandler<UserCreatedDE>
{
    protected override async Task HandleAsync(UserCreatedDE evt, CancellationToken ct)
    {
        // Send push notification
    }
}

public class UserCreatedAnalyticsDEH : DomainEventHandler<UserCreatedDE>
{
    protected override async Task HandleAsync(UserCreatedDE evt, CancellationToken ct)
    {
        // Track analytics
    }
}
```

### Query Handlers

Query Handlers handle read operations with advanced features.

```csharp
using Pulsar.BuildingBlocks.DDD.Mongo.Queries;

public class GetUserQueryHandler
{
    private readonly QueryHandler _queryHandler;
    
    public GetUserQueryHandler(MongoDbSessionFactory sessionFactory)
    {
        _queryHandler = new QueryHandler(sessionFactory, "cluster-name");
    }
    
    public async Task<User?> GetUserAsync(ObjectId userId, CancellationToken ct)
    {
        var collection = _queryHandler.GetCollection<User>(
            "users", 
            ReadPref.SecondaryPreferred);
            
        return await collection
            .Find(_queryHandler.CurrentHandle, u => u.Id == userId)
            .FirstOrDefaultAsync(ct);
    }
    
    public async Task<List<User>> GetActiveUsersAsync(CancellationToken ct)
    {
        var collection = _queryHandler.GetCollection<User>("users");
        
        return await collection
            .Find(_queryHandler.CurrentHandle, u => u.IsActive)
            .SortBy(u => u.LastName)
            .Limit(100)
            .ToListAsync(ct);
    }
}
```

---

## Advanced Features

### Transactions and Isolation Levels

**Using Transactions:**

Transactions are automatically managed by command handlers. Use attributes to control behavior:

```csharp
// No transaction
[NoTransaction]
public class ReadOnlyCH : CommandHandler<ReadOnlyCmd>
{
    protected override async Task HandleAsync(ReadOnlyCmd cmd, CancellationToken ct)
    {
        // No transaction started
    }
}

// With specific isolation level
[WithIsolationLevel(IsolationLevel.ReadCommitted)]
public class ImportantCH : CommandHandler<ImportantCmd>
{
    protected override async Task HandleAsync(ImportantCmd cmd, CancellationToken ct)
    {
        // Transaction with ReadCommitted isolation
    }
}
```

**Manual Transaction Control:**

```csharp
protected override async Task HandleAsync(SomeCmd cmd, CancellationToken ct)
{
    await Session.WithIsolationLevelAsync(async (ct) =>
    {
        // Operations within this block use the specified isolation level
        var user = await userRepository.FindOneByIdAsync(userId, ct);
        user.UpdateStatus();
        await userRepository.ReplaceOneAsync(user, ct: ct);
        
        return 0; // Dummy return value
    }, IsolationLevel.Snapshot, ct);
}
```

**Repository Isolation:**

```csharp
// Create a repository instance with specific isolation level
var isolatedRepo = userRepository.WithIsolation(IsolationLevel.Snapshot);
var user = await isolatedRepo.FindOneByIdAsync(userId);
```

### Retry Policies

**Retry on Exceptions:**

```csharp
// Retry on duplicate key errors
[RetryOnException(DuplicatedKey = true)]
public class CreateUserCH : CommandHandler<CreateUserCmd>
{
    // Will retry if MongoDB duplicate key error occurs
}

// Retry on specific exception types
[RetryOnException(
    ExceptionTypes = new[] { typeof(TimeoutException), typeof(MongoException) },
    Retries = 3)]
public class ResilientCH : CommandHandler<ResilientCmd>
{
    // Will retry up to 3 times on specified exceptions
}

// Custom retry logic
[RetryOnException(Retries = 5)]
public class CustomRetryCH : CommandHandler<CustomCmd>
{
    protected override async Task HandleAsync(CustomCmd cmd, CancellationToken ct)
    {
        // Custom retry logic
        await Session.RetryOnExceptions(async (ct) =>
        {
            // Operation to retry
            return await SomeOperationAsync(ct);
        }, 
        exceptionTypes: new[] { typeof(TemporaryException) },
        retries: 3,
        ct);
    }
}
```

### Causal Consistency

Ensure read-your-writes consistency across operations:

```csharp
[RequiresCausalConsistency]
public class ConsistentCH : CommandHandler<ConsistentCmd>
{
    protected override async Task HandleAsync(ConsistentCmd cmd, CancellationToken ct)
    {
        // Operations here maintain causal consistency
    }
}

// Manual causal consistency
protected override async Task HandleAsync(SomeCmd cmd, CancellationToken ct)
{
    var consistencyToken = await Session.TrackConsistencyToken(async (ct) =>
    {
        // Perform write operation
        await userRepository.InsertOneAsync(newUser, ct);
        return Unit.Value;
    }, ct);
    
    // Use consistency token in subsequent reads
    // to ensure you read your own writes
}
```

### Pagination and Cursors

**Cursor-based Pagination:**

```csharp
using Pulsar.BuildingBlocks.DDD.Mongo.Cursors;

// Create paginator
var paginator = Paginator
    .For<User>(userCollection)
    .OrderBy(u => u.LastName)
    .ThenBy(u => u.FirstName)
    .WithPageSize(20);

// Get first page
var page1 = await paginator.GetPageAsync(cursor: null);

// Get next page using cursor
var page2 = await paginator.GetPageAsync(cursor: page1.NextCursor);

// Results
foreach (var user in page1.Results)
{
    Console.WriteLine(user.FullName);
}

Console.WriteLine($"Has more pages: {page1.HasMore}");
```

**Advanced Pagination:**

```csharp
var paginator = Paginator
    .For<User>(userCollection)
    .Where(u => u.IsActive)
    .OrderBy(u => u.CreatedAt)
    .WithPageSize(50)
    .WithForToken(forToken); // For token ensures consistent pagination

var page = await paginator.GetPageAsync(cursor);
```

### Validation

Validation is automatically applied using FluentValidation:

```csharp
using FluentValidation;

public class CreateOrderCmdValidator : AbstractValidator<CreateOrderCmd>
{
    public CreateOrderCmdValidator(IUserRepository userRepository)
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .MustAsync(async (id, ct) =>
            {
                var user = await userRepository.FindOneByIdAsync(
                    id.ToObjectId(), ct);
                return user != null;
            })
            .WithMessage("Customer not found");
            
        RuleFor(x => x.Items)
            .NotEmpty()
            .Must(items => items.Count > 0)
            .WithMessage("Order must have at least one item");
            
        RuleForEach(x => x.Items)
            .SetValidator(new OrderItemValidator());
    }
}

public class OrderItemValidator : AbstractValidator<OrderItemDto>
{
    public OrderItemValidator()
    {
        RuleFor(x => x.Quantity)
            .GreaterThan(0);
            
        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0);
    }
}
```

Validation errors are automatically converted to `DomainException` with the key `Common.Validation`.

### Audit Information

Track who created and modified aggregates:

```csharp
public class AuditInfo : ValueObject
{
    public DateTime CriadoEm { get; set; }
    public ObjectId? CriadoPorUsuarioId { get; set; }
    public DateTime? ModificadoEm { get; set; }
    public ObjectId? ModificadoPorUsuarioId { get; set; }
    
    public static AuditInfo Create(ObjectId? userId = null)
    {
        return new AuditInfo
        {
            CriadoEm = DateTime.UtcNow,
            CriadoPorUsuarioId = userId
        };
    }
    
    public void MarkModified(ObjectId? userId = null)
    {
        ModificadoEm = DateTime.UtcNow;
        ModificadoPorUsuarioId = userId;
    }
}

// Usage in aggregate
public class Document : AggregateRoot
{
    public AuditInfo AuditInfo { get; set; }
    
    public Document(string title, ObjectId createdByUserId)
    {
        Id = ObjectId.GenerateNewId();
        Title = title;
        AuditInfo = AuditInfo.Create(createdByUserId);
    }
    
    public void Update(string title, ObjectId modifiedByUserId)
    {
        Title = title;
        AuditInfo.MarkModified(modifiedByUserId);
    }
}
```

---

## Best Practices

### 1. Keep Aggregates Small

```csharp
// ? Bad - Large aggregate with too many responsibilities
public class Company : AggregateRoot
{
    public List<Employee> Employees { get; set; } // Could be thousands
    public List<Department> Departments { get; set; }
    public List<Project> Projects { get; set; }
}

// ? Good - Focused aggregates
public class Company : AggregateRoot
{
    public string Name { get; set; }
    public Address Address { get; set; }
}

public class Employee : AggregateRoot
{
    public ObjectId CompanyId { get; set; }
    public string Name { get; set; }
    public ObjectId DepartmentId { get; set; }
}
```

### 2. Use Domain Events for Cross-Aggregate Communication

```csharp
// ? Good - Use events to maintain consistency
public class Order : AggregateRoot
{
    public void Complete()
    {
        Status = OrderStatus.Completed;
        AddDomainEvent(new OrderCompletedDE(Id, CustomerId, TotalAmount));
    }
}

public class OrderCompletedDEH : DomainEventHandler<OrderCompletedDE>
{
    protected override async Task HandleAsync(OrderCompletedDE evt, CancellationToken ct)
    {
        // Update customer statistics in a separate transaction
        var customer = await customerRepository.FindOneByIdAsync(evt.CustomerId);
        customer.IncrementCompletedOrders();
        await customerRepository.ReplaceOneAsync(customer);
    }
}
```

### 3. Validate in Aggregates AND Commands

```csharp
// Command validation (input validation)
public class CreateUserCmdValidator : AbstractValidator<CreateUserCmd>
{
    public CreateUserCmdValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

// Domain validation (business rules)
public class User : AggregateRoot
{
    public void ChangeEmail(string newEmail)
    {
        if (string.IsNullOrWhiteSpace(newEmail))
            throw new DomainException("User.EmailRequired");
            
        if (!IsValidEmail(newEmail))
            throw new DomainException("User.InvalidEmail");
            
        Email = newEmail;
    }
}
```

### 4. Use Specifications for Complex Queries

```csharp
// ? Good - Reusable and testable
public class FindEligibleUsersSpec : IFindSpecification<User>
{
    private readonly DateTime _registeredAfter;
    
    public FindEligibleUsersSpec(DateTime registeredAfter)
    {
        _registeredAfter = registeredAfter;
    }
    
    public FindSpecification<User> GetSpec()
    {
        return new FindSpecification<User>(
            predicate: u => u.IsActive 
                && !u.IsDeleted 
                && u.EmailVerified 
                && u.CreatedAt > _registeredAfter,
            orderBy: new[] { Ordering<User>.Desc(u => u.CreatedAt) },
            skip: null,
            limit: null
        );
    }
}
```

### 5. Handle Optimistic Concurrency

```csharp
protected override async Task HandleAsync(UpdateUserCmd cmd, CancellationToken ct)
{
    try
    {
        var user = await userRepository.FindOneByIdAsync(cmd.UserId.ToObjectId());
        
        user.Update(cmd.FirstName, cmd.LastName);
        
        // Will throw VersionConcurrencyException if version changed
        await userRepository.ReplaceOneAsync(user, checkModified: true);
    }
    catch (VersionConcurrencyException)
    {
        // Handle concurrency conflict
        throw new DomainException("User.ConcurrentUpdate");
    }
}
```

### 6. Use Proper Isolation Levels

```csharp
// Read-only operations - no transaction needed
[NoTransaction]
public class GetUsersCH : CommandHandler<GetUsersCmd, List<UserDto>>
{
    // ...
}

// Critical operations - use snapshot isolation
[WithIsolationLevel(IsolationLevel.Snapshot)]
public class TransferFundsCH : CommandHandler<TransferFundsCmd>
{
    // ...
}
```

### 7. Create Custom Repository Methods When Needed

```csharp
public interface IOrderRepository : IRepository<IOrderRepository, Order>
{
    Task<List<Order>> FindPendingOrdersForCustomerAsync(
        ObjectId customerId, 
        CancellationToken ct = default);
}

public class OrderMongoRepository : MongoRepository<IOrderRepository, Order>, 
                                    IOrderRepository
{
    public async Task<List<Order>> FindPendingOrdersForCustomerAsync(
        ObjectId customerId, 
        CancellationToken ct = default)
    {
        var spec = new FindSpecification<Order>(
            predicate: o => o.CustomerId == customerId 
                && o.Status == OrderStatus.Pending,
            orderBy: new[] { Ordering<Order>.Desc(o => o.CreatedAt) },
            skip: null,
            limit: 50
        );
        
        return await FindManyAsync(spec, ct);
    }
    
    // Standard implementation
    protected override string CollectionName => "orders";
    
    protected override IOrderRepository Clone(MongoDbSession? session, 
                                              MongoDbSessionFactory sessionFactory)
    {
        return new OrderMongoRepository(session, sessionFactory);
    }
}
```

### 8. Use Value Objects for Complex Types

```csharp
// ? Good - Encapsulate related data and behavior
public class PhoneNumber : ValueObject
{
    public string CountryCode { get; }
    public string Number { get; }
    
    public PhoneNumber(string countryCode, string number)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
            throw new ArgumentException("Country code is required");
            
        if (string.IsNullOrWhiteSpace(number))
            throw new ArgumentException("Number is required");
            
        CountryCode = countryCode;
        Number = number.Replace(" ", "").Replace("-", "");
    }
    
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return CountryCode;
        yield return Number;
    }
    
    public string ToFormattedString() => $"+{CountryCode} {Number}";
}
```

### 9. Leverage Domain Events for Integration

```csharp
public class OrderCompletedDEH : DomainEventHandler<OrderCompletedDE>
{
    private readonly ISaveIntegrationEventLog _eventLog;
    
    protected override async Task HandleAsync(OrderCompletedDE evt, CancellationToken ct)
    {
        // Save integration event for other services
        await _eventLog.SaveEventAsync(new OrderCompletedIE
        {
            OrderId = evt.OrderId.ToString(),
            CustomerId = evt.CustomerId.ToString(),
            TotalAmount = evt.TotalAmount
        });
    }
}
```

### 10. Structure Your Projects Properly

```
YourService/
??? YourService.Contracts/          # Commands, queries, DTOs
?   ??? Commands/
?   ??? Queries/
?   ??? IntegrationEvents/
??? YourService.Domain/             # Domain models, events, interfaces
?   ??? Aggregates/
?   ??? Events/
?   ??? Specifications/
?   ??? Exceptions/
??? YourService.Infrastructure/     # Repository implementations
?   ??? Repositories/
??? YourService.API/                # Command/query handlers, controllers
    ??? Application/
        ??? Commands/
        ??? Queries/
        ??? BaseTypes/
```

---

## Summary

The DDD Building Blocks provide a comprehensive framework for building domain-driven microservices with:

- ? **Strong domain modeling** with aggregates, entities, and value objects
- ? **CQRS support** with command and query handlers
- ? **Event-driven architecture** with domain events
- ? **Robust persistence** with MongoDB and the repository pattern
- ? **Transaction management** with configurable isolation levels
- ? **Automatic validation** with FluentValidation
- ? **Optimistic concurrency** control
- ? **Retry policies** for resilience
- ? **Causal consistency** for distributed scenarios

By following these patterns and best practices, you can build maintainable, scalable, and testable microservices that truly reflect your business domain.

For more examples, check the existing services in the solution (Identity, Facility, Catalog, PushNotification).
