namespace Pulsar.Services.Identity.Functions.Application.BaseTypes;

public class FacilityFunction(IMediator mediator)
{
    protected IMediator Mediator => mediator;
}
