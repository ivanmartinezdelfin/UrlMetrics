using System.ComponentModel.DataAnnotations;

namespace UrlMetrics.Api.Dtos
{
    public class CreateLinkRequest
    {
        [Required, Url, MaxLength(2048)]
        public string Url { get; set; } = default;

        // opcional: permitir enviar un shortcode proppio (si está libre
        [MaxLength(12)]
        public string? CustomCode { get; set; }
    }
}