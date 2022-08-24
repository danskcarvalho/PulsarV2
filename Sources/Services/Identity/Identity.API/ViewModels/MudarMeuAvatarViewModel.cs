using System.ComponentModel.DataAnnotations;

namespace Pulsar.Services.Identity.API.ViewModels
{
    public class MudarMeuAvatarViewModel
    {
        /// <summary>
        /// Nova imagem representando o avatar do usuário.
        /// </summary>
        [Required]
        public IFormFile? Imagem { get; set; }

        public UnsupportedMediaTypeResult? Validate()
        {
            if (this.Imagem == null)
                return new UnsupportedMediaTypeResult();
            if (!this.Imagem.FileName.IsValidExtension(".jpg", ".jpeg", ".png"))
                return new UnsupportedMediaTypeResult();

            return null;
        }
    }
}
