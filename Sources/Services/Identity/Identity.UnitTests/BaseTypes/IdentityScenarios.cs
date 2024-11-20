using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Pulsar.Services.Identity.Domain.Aggregates.Dominios;
using Pulsar.Services.Identity.Domain.Aggregates.Grupos;
using Pulsar.Services.Identity.Domain;
using Pulsar.Services.Facility.Contracts.Shadows;
using Pulsar.BuildingBlocks.Sync.Contracts;
using Pulsar.Services.Identity.Functions.Application.Synchronizations;

namespace Identity.UnitTests.BaseTypes;

public partial class IdentityScenarios : IDisposable
{
    private IServiceScope? _scope;
    private IServiceProvider _root;
    protected IMockedDatabase Database { get; private set; }
    protected IServiceProvider Services => _scope?.ServiceProvider ?? throw new InvalidOperationException("no scope");
    protected UsersRegistry Users => new UsersRegistry();

    protected IMockedCollection<Convite> Convites => Database.GetCollection<Convite>(Constants.CollectionNames.CONVITES);
    protected IMockedCollection<Usuario> Usuarios => Database.GetCollection<Usuario>(Constants.CollectionNames.USUARIOS);
    protected IMockedCollection<Dominio> Dominios => Database.GetCollection<Dominio>(Constants.CollectionNames.DOMINIOS);
    protected IMockedCollection<EstabelecimentoShadow> Estabelecimentos => Database.GetCollection<EstabelecimentoShadow>(Shadow<EstabelecimentoShadow>.GetCollectionName());
    protected IMockedCollection<RedeEstabelecimentosShadow> RedesEstabelecimentos => Database.GetCollection<RedeEstabelecimentosShadow>(Shadow<RedeEstabelecimentosShadow>.GetCollectionName());
    protected IMockedCollection<Grupo> Grupos => Database.GetCollection<Grupo>(Constants.CollectionNames.GRUPOS);

    public IdentityScenarios()
    {
        var collection = new ServiceCollection();
        var db = this.Database = Mock.Database();
        IdentityDatabase.Seed(db);
        var configuration = new ConfigurationManager();
        
        var jsonPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new InvalidOperationException("no executing path");
        jsonPath = Path.Combine(jsonPath, "appsettings.json");
        configuration.AddJsonFile(jsonPath, optional: false, reloadOnChange: false);

        collection.AddSingleton<IConfiguration>(configuration);
        collection.AddMockedMongoDB(db, 
            typeof(IdentityScenarios).Assembly,
            // shadows
            typeof(EstabelecimentoShadow).Assembly
            );
        collection.AddMockedEmails();
        collection.AddMockedFileSystem();
        collection.AddTransient<IdentityControllerContext>();
        collection.AddSingleton<IImageManipulationProvider, SkiaSharpImageManipulationProvider>();
        collection.AddTransient(typeof(IdentityCommandHandlerContext<>));
        collection.AddTransient(typeof(IdentityDomainEventHandlerContext<>));
        collection.AddTransient(typeof(IdentityCommandHandlerContext<,>));
        // -- for functions
        collection.AddTransient(typeof(Pulsar.Services.Identity.Functions.Application.BaseTypes.IdentityCommandHandlerContext<>));
        collection.AddTransient(typeof(Pulsar.Services.Identity.Functions.Application.BaseTypes.IdentityDomainEventHandlerContext<>));
        collection.AddTransient(typeof(Pulsar.Services.Identity.Functions.Application.BaseTypes.IdentityCommandHandlerContext<,>));

        collection.AddLogging(c =>
        {
            c.AddConsole();
        });
        collection.AddMediatR(c =>
        {
            c.RegisterServicesFromAssembly(typeof(IdentityController).Assembly);
            c.RegisterServicesFromAssembly(typeof(SyncFacilityFN).Assembly);
        });
        AddMockedQueries(collection);
        AddControllers(collection);
        AddFunctions(collection);

        _root = collection.BuildServiceProvider(true);
        _scope = _root.CreateScope();
    }

    protected TController CreateController<TController>(ClaimsPrincipal? user = null) where TController : IdentityController
    {
        var controller = Services.GetRequiredService<TController>();
        if (user != null)
        {
            var context = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user
                }
            };
            controller.ControllerContext = context;
        }
        return controller;
    }

    protected TFunction CreateFunction<TFunction>() where TFunction : notnull
    {
        return Services.GetRequiredService<TFunction>();
    }

    private void AddMockedQueries(ServiceCollection collection)
    {
        collection.AddTransient<IUsuarioQueries, MockedUsuarioQueries>();
        collection.AddTransient<IDominioQueries, MockedDominioQueries>();
        collection.AddTransient<IEstabelecimentoQueries, MockedEstabelecimentoQueries>();
        collection.AddTransient<IGrupoQueries, MockedGrupoQueries>();
    }

    private void AddControllers(ServiceCollection collection)
    {
        collection.AddTransient<ConviteController>();
        collection.AddTransient<DominioController>();
        collection.AddTransient<EstabelecimentoController>();
        collection.AddTransient<GrupoController>();
        collection.AddTransient<UsuarioController>();
        collection.AddTransient<AceitarConviteController>();
    }

    private void AddFunctions(ServiceCollection collection)
    {
    }

    public void Dispose()
    {
        if (_scope != null)
        {
            _scope.Dispose();
            _scope = null;
        }
    }
}
