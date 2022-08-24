using System.ComponentModel.DataAnnotations;

namespace Pulsar.Services.Identity.API.ViewModels
{
    public class MudarMeuAvatarViewModel
    {
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
