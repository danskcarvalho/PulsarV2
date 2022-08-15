using Pulsar.BuildingBlocks.Emails.Abstractions;

namespace Pulsar.Services.Identity.API.ViewModels;

[EmailModel("Emails/ResetPassword", SubjectProperty = "Subject", ToProperty = "To")]
public class ResetPasswordViewModel
{
    public string To { get; set; }
    public string Subject => "Redefinir Senha";
    public string UserName { get; set; }
    public string Link { get; set; }

    public ResetPasswordViewModel(string to, string userName, string link)
    {
        To = to;
        UserName = userName;
        Link = link;
    }
}
