namespace Pulsar.BuildingBlocks.EmailsSES;

public static class DIExtensions
{
    public static void AddSESEmailSupport(this IServiceCollection col)
    {
        col.AddTransient<IViewRenderService, RazorViewRenderService>();
        col.AddTransient<IEmailService, SESEmailService>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var fromName = config["AmazonSES:FromName"]!;
            var fromEmail = config["AmazonSES:FromEmail"]!;
            var awsProfile = config["AmazonSES:AwsProfile"]!;
            var region = config["AmazonSES:Region"]!;

            var options = new SESConfigurationSection(fromName, fromEmail, awsProfile, region);
            var renderer = sp.GetRequiredService<IViewRenderService>();
            var logger = sp.GetRequiredService<ILogger<SESEmailService>>();
            return new SESEmailService(options, renderer, logger);
        });
    }
}
