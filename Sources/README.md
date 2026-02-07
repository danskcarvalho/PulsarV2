# Pulsar V2

Pulsar V2 is a modern microservices-based application built with .NET 9, showcasing enterprise-grade architecture patterns and best practices.

## ??? Architecture

This solution implements a **microservices architecture** with the following key components:

- **Frontend**: Blazor WebAssembly application
- **Backend Services**: Multiple domain-driven microservices (Identity, Facility, Catalog, Push Notifications)
- **Orchestration**: .NET Aspire for service orchestration and local development
- **Event-Driven**: Event dispatchers and Azure Functions for asynchronous processing
- **Data**: MongoDB for persistence with DDD patterns

## ?? Technologies

- **.NET 9** - Latest .NET framework
- **.NET Aspire** - Cloud-native orchestration
- **Blazor WebAssembly** - Modern web UI framework
- **Azure Functions** - Serverless compute
- **MongoDB** - Document database
- **Redis** - Distributed caching
- **MediatR** - CQRS and messaging patterns
- **Duende BFF** - Backend for Frontend security
- **OpenID Connect** - Authentication
- **FluentUI** - Microsoft Fluent UI components

## ?? Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (for running dependencies)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (17.12 or later) or [Visual Studio Code](https://code.visualstudio.com/)
- [MongoDB](https://www.mongodb.com/try/download/community) (or use Docker)
- [Redis](https://redis.io/download) (or use Docker)
- [Azure Functions Core Tools](https://docs.microsoft.com/azure/azure-functions/functions-run-local) (for local function development)

## ?? Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/danskcarvalho/PulsarV2.git
cd PulsarV2
```

### 2. Start Dependencies

The easiest way to run dependencies is using .NET Aspire, which will automatically start MongoDB and Redis:

```bash
cd Orchestration/Pulsar.AppHost
dotnet run
```

Alternatively, start dependencies manually with Docker:

```bash
# MongoDB
docker run -d -p 27017:27017 --name pulsar-mongo mongo:latest

# Redis
docker run -d -p 6379:6379 --name pulsar-redis redis:latest
```

### 3. Configure User Secrets

Set up user secrets for local development:

```bash
cd Orchestration/Pulsar.AppHost
dotnet user-secrets init
dotnet user-secrets set "MongoDB:ConnectionStringName" "MongoDb"
dotnet user-secrets set "ConnectionStrings:MongoDb" "mongodb://localhost:27017"
```

### 4. Run Migrations

Apply database migrations for each service:

```bash
dotnet run --project Migrations/ServiceBus.Migrations
dotnet run --project Services/Identity/Identity.Migrations
dotnet run --project Services/Facility/Facility.Migrations
dotnet run --project Services/PushNotification/PushNotification.Migrations
dotnet run --project Services/Catalog/Catalog.Migrations
```

### 5. Launch the Application

Run the Aspire AppHost to start all services:

```bash
cd Orchestration/Pulsar.AppHost
dotnet run
```

The Aspire dashboard will open in your browser, showing all running services and their endpoints.

## ?? Project Structure

```
PulsarV2/
??? BuildingBlocks/           # Shared building blocks and infrastructure
?   ??? Caching/              # Caching abstractions and implementations
?   ??? DDD/                  # Domain-Driven Design base classes
?   ??? EventBus/             # Event bus contracts and implementations
?   ??? FileSystem/           # File system abstractions (Azure Blob)
?   ??? Migrations/           # Migration framework
?   ??? Sync/                 # Synchronization patterns
?   ??? Utils/                # Utility libraries
??? Frontends/
?   ??? Web/Pulsar.Web/       # Blazor WebAssembly frontend
??? Migrations/               # Shared migrations (Service Bus)
??? Orchestration/
?   ??? Pulsar.AppHost/       # .NET Aspire orchestration
?   ??? Pulsar.ServiceDefaults/ # Shared service defaults
??? Services/                 # Microservices
?   ??? Catalog/              # Catalog service
?   ??? Facility/             # Facility management service
?   ??? Identity/             # Identity and authentication service
?   ??? PushNotification/     # Push notification service
??? README.md
```

### Service Structure

Each service follows a consistent structure:

- **{Service}.API** - REST API endpoints
- **{Service}.Contracts** - DTOs, commands, and queries
- **{Service}.Domain** - Domain models and business logic
- **{Service}.Infrastructure** - Data access and external integrations
- **{Service}.Functions** - Azure Functions for background processing
- **{Service}.EventDispatcher** - Event handling workers
- **{Service}.Migrations** - Database migrations

## ??? Development

### Building the Solution

```bash
dotnet build
```

### Running Tests

```bash
dotnet test
```

### Code Style

The project follows standard .NET conventions:
- C# 12 language features
- Nullable reference types enabled
- Implicit usings enabled

### Domain-Driven Design

The solution implements DDD patterns:
- **Aggregates** - Consistency boundaries
- **Entities** - Objects with identity
- **Value Objects** - Immutable objects without identity
- **Domain Events** - Domain state changes
- **Repositories** - Data access abstractions
- **CQRS** - Command Query Responsibility Segregation

?? **[Read the complete DDD Building Blocks Guide](BuildingBlocks/DDD/README.md)** - Comprehensive documentation with code examples and best practices for using the DDD libraries in your microservices.

## ?? Security

- **Authentication**: OpenID Connect with Duende BFF
- **Authorization**: Role-based and policy-based
- **API Security**: Backend for Frontend (BFF) pattern

## ?? Configuration

Key configuration settings:

- **MongoDB**: Connection string and database name
- **Redis**: Cache configuration
- **OpenID Connect**: Identity provider settings
- **Azure Functions**: Service bus and storage connections

Configuration can be provided via:
- `appsettings.json`
- `appsettings.Development.json`
- User secrets (for local development)
- Environment variables (for production)

## ?? Troubleshooting

### MongoDB Connection Issues

Ensure MongoDB is running and accessible:

```bash
docker ps | grep mongo
```

### Redis Connection Issues

Verify Redis is running:

```bash
docker ps | grep redis
```

### Build Errors After Upgrade

Clean and rebuild the solution:

```bash
dotnet clean
dotnet build
```

## ?? Additional Resources

- [.NET 9 Documentation](https://docs.microsoft.com/dotnet/core/whats-new/dotnet-9)
- [.NET Aspire Documentation](https://learn.microsoft.com/dotnet/aspire/)
- [Blazor Documentation](https://docs.microsoft.com/aspnet/core/blazor/)
- [MongoDB .NET Driver](https://docs.mongodb.com/drivers/csharp/)
- [Duende BFF Documentation](https://docs.duendesoftware.com/identityserver/v6/bff/)

## ?? Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## ?? License

This project is licensed under the MIT License - see the LICENSE file for details.

## ?? Authors

- **Dansk Carvalho** - *Initial work* - [danskcarvalho](https://github.com/danskcarvalho)

## ?? Acknowledgments

- Built with .NET 9 and .NET Aspire
- Inspired by modern microservices architecture patterns
- Uses Domain-Driven Design principles
