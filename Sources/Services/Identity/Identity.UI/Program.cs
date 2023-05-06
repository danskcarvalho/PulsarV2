using Pulsar.Services.Identity.UI;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Pulsar.Services.Identity.UI.Clients;
using Pulsar.Services.Identity.UI.Clients.Interfaces;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<ILoginClient, LoginClient>();
builder.Services.AddScoped<ILogoutClient, LogoutClient>();
builder.Services.AddScoped<IEsqueciMinhaSenhaClient, EsqueciMinhaSenhaClient>();
builder.Services.AddScoped<IAceitarConviteClient, AceitarConviteClient>();

await builder.Build().RunAsync();
