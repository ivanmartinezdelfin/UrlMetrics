using System.ComponentModel.DataAnnotations;

namespace UrlMetrics.Api.Dtos
{
    public class CreateLinkRequest
    {
        [Required, Url, MaxLength(2048)]
        public string Url { get; set; } = string.Empty;

        // opcional: permitir enviar un shortcode propio (si está libre
        [MaxLength(12)]
        public string? CustomCode { get; set; }
    }
}