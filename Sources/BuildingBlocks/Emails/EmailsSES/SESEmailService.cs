namespace Pulsar.BuildingBlocks.EmailsSES;

public class SESEmailService : IEmailService
{
    public SESConfigurationSection Configuration { get; }
    private readonly IViewRenderService _viewRenderService;
    private readonly ILogger<SESEmailService> _logger;

    public SESEmailService(SESConfigurationSection configuration, IViewRenderService viewRenderService, ILogger<SESEmailService> logger)
    {
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _viewRenderService = viewRenderService ?? throw new ArgumentNullException(nameof(viewRenderService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Send<TModel>(TModel model, CancellationToken ct = default, bool throwOnError = false) where TModel : class
    {
        var retryPolicy = Policy
                   .Handle<Exception>(e => true)
                   .WaitAndRetryAsync(3,
                       retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt) * 0.5 + Math.Pow(2, retryAttempt) * Random.Shared.NextDouble()),
                       (e, ts) =>
                       {
                           _logger.LogError(e, "error while sending e-mail");
                           _logger.LogInformation($"will retry in {(int)ts.TotalSeconds} seconds");
                       });

        try
        {
            var chain = new CredentialProfileStoreChain();
            CredentialProfile pulsarProfile;
            if (!chain.TryGetProfile("pulsar", out pulsarProfile))
                throw new InvalidOperationException("no pulsar profile, use 'aws --profile pulsar configure' to set up a profile");
            AWSCredentials awsCredentials;
            if (!chain.TryGetAWSCredentials("pulsar", out awsCredentials))
                throw new InvalidOperationException("no pulsar profile, use 'aws --profile pulsar configure' to set up a profile");

            var region = pulsarProfile.Region;
            if (region == null)
                region = Amazon.RegionEndpoint.SAEast1;

            if (Configuration.Region != null)
                region = Amazon.RegionEndpoint.GetBySystemName(Configuration.Region);

            await retryPolicy.ExecuteAsync(async (CancellationToken ct2) =>
            {
                using var client = new AmazonSimpleEmailServiceClient(awsCredentials, region);
                var attribute = EmailModelAttribute.GetAttribute(typeof(TModel));
                if (attribute is null)
                    throw new InvalidOperationException("no e-mail attribute");
                var to = attribute.GetTo(model);
                var subject = attribute.GetSubject(model);
                if (to is null)
                    throw new InvalidOperationException("to is null");
                if (subject is null)
                    throw new InvalidOperationException("subject is null");

                var htmlBody = await _viewRenderService.RenderToStringAsync(attribute.View, model);
                var sendRequest = new SendEmailRequest
                {
                    Source = !string.IsNullOrWhiteSpace(Configuration.FromName) ?
                       new MailAddress(Configuration.FromEmail, Configuration.FromName).ToString() : Configuration.FromEmail,
                    Destination = new Destination
                    {
                        ToAddresses =
                            new List<string> { to }
                    },
                    Message = new Message
                    {
                        Subject = new Content(subject),
                        Body = new Body
                        {
                            Html = new Content
                            {
                                Charset = "UTF-8",
                                Data = htmlBody
                            }
                        }
                    }
                };

                await client.SendEmailAsync(sendRequest, ct2);
            }, ct);
        }
        catch(Exception e)
        {
            _logger.LogError(e, "error while sending e-mail");
            if (throwOnError)
                throw;
        }
    }
}

public class SESConfigurationSection
{
    public string? FromName { get; private set; }
    public string FromEmail { get; private set; }
    public string AwsProfile { get; private set; }
    public string? Region { get; private set; }

    public SESConfigurationSection(string? fromName, string fromEmail, string awsProfile, string? region)
    {
        FromName = fromName;
        FromEmail = fromEmail;
        AwsProfile = awsProfile;
        Region = region;
    }
}

public class SESConfigurationSectionBuilder
{
    public string? FromName { get; set; }
    public string? FromEmail { get; set; }
    public string? AwsProfile { get; set; }
    public string? Region { get; set; }

    public SESConfigurationSection Build() => new SESConfigurationSection(
        FromName, 
        FromEmail ?? throw new InvalidOperationException("FromEmail is null"),
        AwsProfile ?? throw new InvalidOperationException("AwsProfile is null"),
        Region);
}