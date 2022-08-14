using Pulsar.Services.Identity.Contracts.DTOs;
using System.Runtime.Serialization;

namespace Pulsar.Services.Identity.UI.Models
{
    public class BackendException : Exception
    {
        public ExceptionDTO Details { get; }

        public BackendException(ExceptionDTO details) : base(GetMessage(details))
        {
            Details = details;
        }

        private static string GetMessage(ExceptionDTO details)
        {
            if (details.Types.Any(t => t.Contains("DomainException")))
                return details.Message;
            else
                return "Erro interno do servidor. Tente novamente mais tarde.";
        }
    }
}
