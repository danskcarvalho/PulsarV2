using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;
using Pulsar.Services.Facility.Contracts.Commands.Estabelecimentos;
using Pulsar.Services.Identity.Contracts.Commands.Convites;
using Pulsar.Services.PushNotification.Contracts.Commands.PushNotifications;
using Pulsar.Web.Client.Clients;
using Pulsar.Web.Client.Services.Authentication;
using Pulsar.Web.Client.Services.Others;
using Pulsar.Web.Client.Services.PushNotifications;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddFluentUIComponents();
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddSingleton<AuthenticationStateProvider, PulsarAuthenticationStateProvider>();

// HTTP client configuration
builder.Services.AddTransient<AntiforgeryHandler>();

builder.Services.AddHttpClient("backend", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<AntiforgeryHandler>();
builder.Services.AddTransient(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("backend"));
builder.Services.AddClients();
builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<ConsistencyTokenManager>();
builder.Services.AddTransient<ProtectedSectionService>();
builder.Services.AddBlazoredSessionStorage();
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(typeof(Program).Assembly);
});
builder.Services.AddPushNotificationServices(config =>
{
    config.AddAssembliesToScanForRoutingActions(typeof(Program).Assembly);
    config.AddAssembliesToScanForIntegrationEvents(
        // facility contracts
        typeof(CriarEstabelecimentoCmd).Assembly,
		// identity contracts
		typeof(CriarConviteCmd).Assembly,
		// push notification contracts
		typeof(CriarNotificacaoPushCmd).Assembly);
});

await builder.Build().RunAsync();
