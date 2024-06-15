using Microsoft.AspNetCore.Diagnostics;
using Pulsar.BuildingBlocks.DDD;
using Pulsar.Services.Shared.DTOs;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pulsar.Services.Shared.API.Services;

public static class JsonExceptionMiddleware
{
    public static void UseJsonExceptionMiddleware(this WebApplication app)
    {
        app.UseExceptionHandler(new ExceptionHandlerOptions()
        {
            ExceptionHandler = JsonExceptionMiddleware.Invoke
        });
    }

    public static async Task Invoke(HttpContext context)
    {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var ex = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        if (ex == null) return;

        if (ex is DomainException)
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

        var error = new ExceptionDTO
        (
            GetTypes(ex),
            GetKey(ex),
            ex.Message
        );

        context.Response.ContentType = "application/json";

        var options = new JsonSerializerOptions();
        options.Converters.Add(new JsonStringEnumConverter());
        await JsonSerializer.SerializeAsync(context.Response.Body, error, error.GetType(), options);
    }

    private static string? GetKey(Exception ex)
    {
        if (ex is DomainException ie)
            return ie.StringKey;
        else
            return null;
    }

    private static List<string> GetTypes(Exception ex)
    {
        List<string> r = new List<string>();
        var baseType = ex.GetType();
        while (baseType is not null && baseType.FullName is not null)
        {
            r.Add(baseType.FullName);
            baseType = baseType.BaseType;
        }
        return r;
    }
}
