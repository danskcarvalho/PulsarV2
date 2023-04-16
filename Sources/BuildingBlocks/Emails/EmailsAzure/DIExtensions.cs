using Pulsar.BuildingBlocks.Emails;

namespace Pulsar.BuildingBlocks.FileSystemAzure;

public static class DIExtensions
{
    public static void AddAzureEmails(this IServiceCollection col)
    {
        col.AddTransient<IViewRenderService, RazorViewRenderService>();
        col.AddTransient<IEmailService>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var logger = sp.GetRequiredService<ILogger<AzureEmailService>>();
            var renderer = sp.GetRequiredService<IViewRenderService>();

            return new AzureEmailService(
                logger,
                renderer,
                config.GetOrThrow("Azure:Emails:ServiceUri"),
                config.GetOrThrow("Azure:Emails:Sender"));
        });
    }
}
