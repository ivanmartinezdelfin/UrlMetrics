using System.ComponentModel.DataAnnotations;

namespace UrlMetrics.Api.Dtos
{
    public class UpdateLinkRequest
    {
        [Required, Url, MaxLength(2048)]
        public string Url { get; set; } = default!;

        // Permite activar/desactivar el link
        public bool? IsActive { get; set; }

    }
}