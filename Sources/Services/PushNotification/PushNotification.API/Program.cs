using Pulsar.BuildingBlocks.FileSystemAzure;
using Pulsar.Services.PushNotification.Contracts.Commands.PushNotifications;
using Pulsar.Services.Identity.Contracts.Shadows;
using Pulsar.Services.PushNotification.API.Application.BaseTypes;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.

builder.Services.AddCors(options =>
{
    // this defines a CORS policy called "default"
    options.AddPolicy("CorsDefaultPolicy", policy =>
    {
        HashSet<string> origins = new HashSet<string>();
        origins.UnionWith(builder.Configuration.GetSection("AllowedCorsOrigins").GetChildren().Select(c => c.Value?.FormatUri(builder.Configuration)).Where(c => c != null).ToList()!);
        policy.WithOrigins(origins.ToArray())
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddControllers().AddJsonOptions(j =>
{
    j.JsonSerializerOptions.Converters.Add(new AssertDateTimeConverter());
    j.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddTransient<BaseControllerContext>();
builder.Services.AddTransient(typeof(PushNotificationCommandHandlerContext<>));
builder.Services.AddTransient(typeof(PushNotificationDomainEventHandlerContext<>));
builder.Services.AddTransient(typeof(PushNotificationCommandHandlerContext<,>));

builder.Services.AddQueries();
builder.Services.AddMongoDB(
    typeof(PushNotificationMongoRepository).Assembly,
    // shadow from other services
    typeof(UsuarioShadow).Assembly
    );
builder.Services.AddValidators(typeof(MarcarNotificacoesComoLidaCmd).Assembly);
builder.Services.AddMediatR(c =>
{
    c.RegisterServicesFromAssembly(typeof(Program).Assembly);
});
builder.Services.AddRedisCache();
builder.Services.AddAzureBlobStorage();
builder.Services.AddTransient<PushNotificationQueriesContext>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    return new PushNotificationQueriesContext(
        sp.GetRequiredService<MongoDbSessionFactory>(),
        sp.GetRequiredService<ICacheServer>(),
        configuration.GetOrThrow("MongoDB:ClusterName"));
});
builder.Services.AddSingleton<IAuthorizationPolicyProvider, CustomPolicyProvider>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = builder.Configuration.GetUri("IdentityServer:Authority");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            RequireAudience = true,
            ValidAudience = "pushnotification",
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true
        };
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.CustomSchemaIds(x => GenericTypeExtensions.GetGenericTypeName(x));
    options.SwaggerDoc("v2", new OpenApiInfo
    {
        Title = "Push Notification HTTP API",
        Version = "v2",
        Description = "The Push Notification Service HTTP API"
	});
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows()
        {
            AuthorizationCode = new OpenApiOAuthFlow()
            {
                AuthorizationUrl = new Uri(builder.Configuration.GetUri("IdentityServer:AuthorizationUrl")),
                TokenUrl = new Uri(builder.Configuration.GetUri("IdentityServer:TokenUrl")),
                Scopes = AllApiScopes.Resources.Where(s => s.Name == "pushnotification.*").ToDictionary(s => (s.Name, s.Description))
            }
        }
    });
    options.OperationFilter<AuthorizeOperationFilter>();
    options.SchemaFilter<SwaggerExcludeFilter>();

    // Set the comments path for the Swagger JSON and UI.
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = System.IO.Path.Combine(System.AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
    xmlFile = "PushNotification.Contracts.xml";
    xmlPath = System.IO.Path.Combine(System.AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint($"/swagger/v2/swagger.json", "PushNotification.API v2");
        c.OAuthClientId("pushnotificationswaggerui");
        c.OAuthAppName("Push Notification Swagger UI");
        c.OAuthUsePkce();
        c.InjectStylesheet("/css/swagger.css");
    });
}

app.UseJsonExceptionMiddleware();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCors("CorsDefaultPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
public partial class Program { }
