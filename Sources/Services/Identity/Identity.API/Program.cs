var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews().AddJsonOptions(j =>
{
    j.JsonSerializerOptions.Converters.Add(new AssertDateTimeConverter());
    j.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddRazorPages();
builder.Services.AddQueries();
builder.Services.AddMongoDB(typeof(UsuarioMongoRepository).Assembly);
builder.Services.AddValidators(typeof(EsqueciMinhaSenhaCmd).Assembly);
builder.Services.AddMediatR(c =>
{
    c.RegisterServicesFromAssembly(typeof(Program).Assembly);
});
builder.Services.AddSESEmailSupport();
builder.Services.AddRedisCache();
builder.Services.AddMinio();
builder.Services.AddTransient<IdentityControllerContext>();
builder.Services.AddSingleton<IImageManipulationProvider, SkiaSharpImageManipulationProvider>();
builder.Services.AddTransient(typeof(IdentityCommandHandlerContext<>));
builder.Services.AddTransient(typeof(IdentityDomainEventHandlerContext<>));
builder.Services.AddTransient(typeof(IdentityCommandHandlerContext<,>));
builder.Services.AddTransient<IdentityQueriesContext>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    return new IdentityQueriesContext(
        sp.GetRequiredService<MongoDbSessionFactory>(),
        configuration.GetOrThrow("MongoDB:ClusterName"));
});
builder.Services.AddSingleton<IAuthorizationPolicyProvider, IdentityCustomPolicyProvider>();
builder.Services.AddAuthentication().AddJwtBearer("Bearer", options =>
{
    options.Authority = builder.Configuration.GetOrThrow("IdentityServer:Authority");
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateAudience = false
    };
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.CustomSchemaIds(x => GenericTypeExtensions.GetGenericTypeName(x));
    options.SwaggerDoc("v2", new OpenApiInfo
    {
        Title = "Identity HTTP API",
        Version = "v2",
        Description = "The Identity Service HTTP API"
    });
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows()
        {
            AuthorizationCode = new OpenApiOAuthFlow()
            {
                AuthorizationUrl = new Uri(builder.Configuration.GetOrThrow("IdentityServer:AuthorizationUrl")),
                TokenUrl = new Uri(builder.Configuration.GetOrThrow("IdentityServer:TokenUrl")),
                Scopes = AllApiScopes.Resources.Where(s => s.Name == "identity.*").ToDictionary(s => (s.Name, s.Description))
            }
        }
    });
    options.OperationFilter<AuthorizeOperationFilter>();
    options.SchemaFilter<SwaggerExcludeFilter>();

    // Set the comments path for the Swagger JSON and UI.
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = System.IO.Path.Combine(System.AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
    xmlFile = "Identity.Contracts.xml";
    xmlPath = System.IO.Path.Combine(System.AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});
builder.Services.AddTransient<IProfileService, UserProfileService>();
var idserver = builder.Services.AddIdentityServer(options =>
{
    options.Authentication.CookieLifetime = TimeSpan.FromHours(2);
    options.Authentication.CookieSlidingExpiration = true;

    var keypath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    if (!keypath.EndsWith(Path.DirectorySeparatorChar))
        keypath += Path.DirectorySeparatorChar;
    keypath += "Pulsar" + Path.DirectorySeparatorChar + "Keys";
    options.KeyManagement.KeyPath = keypath;
});
idserver.AddInMemoryClients(AllClients.Resources(builder.Configuration))
    .AddInMemoryApiResources(AllApiResources.Resources)
    .AddInMemoryIdentityResources(AllIdentityResources.Resources)
    .AddInMemoryApiScopes(AllApiScopes.Resources);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseExceptionHandler(new ExceptionHandlerOptions()
{
    ExceptionHandler = JsonExceptionMiddleware.Invoke
});

app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseRouting();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint($"/swagger/v2/swagger.json", "Identity.API v2");
    c.OAuthClientId("identityswaggerui");
    c.OAuthAppName("Identity Swagger UI");
    c.OAuthUsePkce();
    c.InjectStylesheet("/css/swagger.css");
});
app.UseIdentityServer();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.MapDefaultControllerRoute();
app.MapFallbackToFile("index.html");

app.Run();
