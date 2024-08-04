using Duende.Bff.Yarp;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Pulsar.Services.ApiRegistry;
using Pulsar.Web.Client.Pages;
using Pulsar.Web.Components;
using Pulsar.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();
builder.Services.AddFluentUIComponents();
builder.Services.AddBff().AddRemoteApis();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();
builder.Services.AddAuthorization();
if (builder.Environment.IsDevelopment())
{
    // allow all localhost ports
    builder.Services.AddCors(o => o.AddPolicy("BlazorCorsPolicy", b => b.SetIsOriginAllowed(s => new Uri(s).IsLoopback)));
    // or, explicitly allow client's address only
    //services.AddCors(o => o.AddPolicy("BlazorCorsPolicy", b => b.WithOrigins("http://localhost:6001")));
}
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = "cookie";
        options.DefaultChallengeScheme = "oidc";
        options.DefaultSignOutScheme = "oidc";
    })
    .AddCookie("cookie", options =>
    {
        options.Cookie.Name = "__Pulsar_Web";
        options.Cookie.SameSite = SameSiteMode.Strict;
    })
    .AddOpenIdConnect("oidc", options =>
    {
        options.Authority = builder.Configuration["services:identity-api:https:0"];

        options.ClientId = "pulsarweb";
        options.ResponseType = "code";
        //options.UsePkce = true;
        // query response type is compatible with strict SameSite mode
        options.ResponseMode = OpenIdConnectResponseMode.Query;

        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("usuario_admin");
        options.Scope.Add("dominio_logado");
        options.Scope.Add("dominio_estabelecimento_logado");
        options.Scope.Add("estabelecimento_logado");
        options.Scope.Add("dominio_logado_perms");
        options.Scope.Add("estabelecimento_logado_perms");
        options.Scope.Add("identity.*");
        options.Scope.Add("catalog.*");
        options.Scope.Add("offline_access");

        options.MapInboundClaims = false;
        options.GetClaimsFromUserInfoEndpoint = true;
        options.SaveTokens = true;
        options.DisableTelemetry = true;
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.MapDefaultEndpoints();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("BlazorCorsPolicy");
app.UseAuthentication();
app.UseAntiforgery();
app.UseBff();
app.UseAuthorization();
app.MapBffManagementEndpoints();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Pulsar.Web.Client._Imports).Assembly);
app.MapRemoteBffApiEndpoint("/api/identity", builder.Configuration["services:identity-api:https:0"]!).RequireAccessToken(Duende.Bff.TokenType.User);
app.MapRemoteBffApiEndpoint("/api/catalog", builder.Configuration["services:catalog-api:https:0"]!).RequireAccessToken(Duende.Bff.TokenType.User);

app.Run();
