namespace Pulsar.BuildingBlocks.Emails.Abstractions;

public interface IViewRenderService
{
    Task<string> RenderToStringAsync(string viewName, object model);
}
