using System.ComponentModel.DataAnnotations;

namespace Pulsar.Services.Identity.API.ViewModels
{
    public class MudarMeuAvatarViewModel
    {
        private static readonly string[] _validContentTypes = ["image/jpg", "image/jpeg", "image/png"];
        private const int MaxImageSize = 500 * 1024; // 500KB
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
            if (!_validContentTypes.Contains(this.Imagem.ContentType))
                return new UnsupportedMediaTypeResult();
            if (this.Imagem.Length > MaxImageSize)
                return new UnsupportedMediaTypeResult();

            return null;
        }
    }
}
