using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Polly;
using Pulsar.Services.Identity.Domain.Aggregates.Convites;
using Pulsar.Services.Identity.UI.Pages;
using System.Reflection;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

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
        collection.AddMockedMongoDB(db, typeof(IdentityScenarios).Assembly);
        collection.AddMockedEmails();
        collection.AddMockedFileSystem();
        collection.AddTransient<IdentityControllerContext>();
        collection.AddSingleton<IImageManipulationProvider, SkiaSharpImageManipulationProvider>();
        collection.AddTransient(typeof(IdentityCommandHandlerContext<>));
        collection.AddTransient(typeof(IdentityDomainEventHandlerContext<>));
        collection.AddTransient(typeof(IdentityCommandHandlerContext<,>));
        collection.AddLogging(c =>
        {
            c.AddConsole();
        });
        collection.AddMediatR(c =>
        {
            c.RegisterServicesFromAssembly(typeof(IdentityController).Assembly);
        });
        AddMockedQueries(collection);
        AddControllers(collection);

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

    public void Dispose()
    {
        if (_scope != null)
        {
            _scope.Dispose();
            _scope = null;
        }
    }
}
