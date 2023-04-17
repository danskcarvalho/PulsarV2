namespace Pulsar.Services.Identity.Functions.Application.BaseTypes;

public class IdentityFunction
{
    public IdentityFunction(IMediator mediator)
    {
        Mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    protected IMediator Mediator { get; }
}
