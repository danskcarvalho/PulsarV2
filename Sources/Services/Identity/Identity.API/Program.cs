using Duende.IdentityServer.Events;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Polly;
using Pulsar.Services.Identity.API.Filters;
using Pulsar.Services.Identity.API.Services;
using System.Net;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddQueries();
builder.Services.AddMongoDB(typeof(UsuarioMongoRepository).Assembly);
builder.Services.AddMediatR(typeof(UsuarioQueries).Assembly);
builder.Services.AddAuthentication().AddJwtBearer("Bearer", options =>
{
    options.Authority = builder.Configuration["IdentityServer:Authority"];
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateAudience = false
    };
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Identity HTTP API",
        Version = "v1",
        Description = "The Identity Service HTTP API"
    });
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows()
        {
            AuthorizationCode = new OpenApiOAuthFlow()
            {
                AuthorizationUrl = new Uri(builder.Configuration["IdentityServer:AuthorizationUrl"]),
                TokenUrl = new Uri(builder.Configuration["IdentityServer:TokenUrl"]),
                Scopes = new Dictionary<string, string>() {
                    { "identity.read", "Read identity data." },
                    { "identity.write", "Write identity data." }
                }
            }
        }
    });
    options.OperationFilter<AuthorizeOperationFilter>();
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
