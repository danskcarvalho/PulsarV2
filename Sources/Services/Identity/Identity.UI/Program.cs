using Pulsar.Services.Identity.UI;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Pulsar.Services.Identity.UI.Clients;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<LoginClient>();
builder.Services.AddScoped<LogoutClient>();
builder.Services.AddScoped<EsqueciMinhaSenhaClient>();

await builder.Build().RunAsync();
