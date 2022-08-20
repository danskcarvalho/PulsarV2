namespace Pulsar.Services.Identity.API.ViewModels;

[EmailModel("Emails/ConvidarUsuario", SubjectProperty = "Subject", ToProperty = "To")]
public class ConvidarUsuarioViewModel
{
    public string To { get; set; }
    public string Subject => "Pulsar - Convite";
    public string UserName { get; set; }
    public string Link { get; set; }

    public ConvidarUsuarioViewModel(string to, string userName, string link)
    {
        To = to;
        UserName = userName;
        Link = link;
    }
}
