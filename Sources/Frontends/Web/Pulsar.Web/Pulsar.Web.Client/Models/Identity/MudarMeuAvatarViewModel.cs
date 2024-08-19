using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;

namespace Pulsar.Web.Client.Models.Identity;

public class MudarMeuAvatarViewModel
{
    /// <summary>
    /// Nova imagem representando o avatar do usuário.
    /// </summary>
    [Required]
    public IBrowserFile? Imagem { get; set; }
}
