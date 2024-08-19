using Pulsar.Services.Shared.DTOs;
using System.Net;

namespace Pulsar.Web.Client.Exceptions;

public class BackendException : Exception
{
    public HttpStatusCode HttpStatusCode { get; }
    public ExceptionDTO? Details { get; }

    public BackendException(HttpStatusCode httpStatusCode, ExceptionDTO details) : base(GetMessage(details))
    {
        HttpStatusCode = httpStatusCode;
        Details = details;
    }

    public BackendException(HttpStatusCode httpStatusCode) : base("Erro interno do servidor. Tente novamente mais tarde.")
    {
        HttpStatusCode = httpStatusCode;
        Details = null;
    }

    public BackendException(HttpStatusCode httpStatusCode, string message) : base(message)
    {
        HttpStatusCode = httpStatusCode;
        Details = null;
    }

    public BackendException(HttpStatusCode httpStatusCode, string message, Exception innerException) : base(message, innerException)
    {
        HttpStatusCode = httpStatusCode;
        Details = null;
    }

    public BackendException(HttpStatusCode httpStatusCode, ExceptionDTO details, Exception innerException) : base(GetMessage(details), innerException)
    {
        HttpStatusCode = httpStatusCode;
        Details = details;
    }

    public BackendException(HttpStatusCode httpStatusCode, Exception innerException) : base("Erro interno do servidor. Tente novamente mais tarde.", innerException)
    {
        HttpStatusCode = httpStatusCode;
        Details = null;
    }


    private static string GetMessage(ExceptionDTO details)
    {
        if (details.Types.Any(t => t.Contains("DomainException")))
            return details.Message;
        else
            return "Erro interno do servidor. Tente novamente mais tarde.";
    }
}
