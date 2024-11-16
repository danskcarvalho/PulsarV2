namespace Pulsar.Services.Facility.Functions.Application.BaseTypes;

public class FacilityFunction(IMediator mediator)
{
    protected IMediator Mediator => mediator;
}
