using Azure.Communication.Email;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Polly;
using System.Net.Mail;
using System.Reflection;

namespace Pulsar.BuildingBlocks.EmailsAzure;

public class AzureEmailService : IEmailService
{
    private readonly ILogger<AzureEmailService> _logger;
    private readonly IViewRenderService _viewRenderService;
    private readonly EmailClient _emailClient;
    private readonly string _senderAddress;

    public AzureEmailService(ILogger<AzureEmailService> logger, IViewRenderService viewRenderService, 
        string endpoint, 
        string senderAddress)
    {
        _logger = logger;
        _viewRenderService = viewRenderService;
        _emailClient = new EmailClient(new Uri(endpoint), new DefaultAzureCredential());
        _senderAddress = senderAddress;
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
            await retryPolicy.ExecuteAsync(async (CancellationToken ct2) =>
            {
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
                await _emailClient.SendAsync(
                        Azure.WaitUntil.Started,
                        _senderAddress,
                        to,
                        subject,
                        htmlBody,
                        cancellationToken: ct2);
            }, ct);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "error while sending e-mail");
            if (throwOnError)
                throw;
        }
    }
}
