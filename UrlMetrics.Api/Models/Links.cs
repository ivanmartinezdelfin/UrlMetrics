using System;
using System.ComponentModel.DataAnnotations;

namespace UrlMetrics.Api.Models
{
    public class Links
    {
        public int Id { get; set; }
        [Required, MaxLength(12)]
        public string ShortCode { get; set; } = default!; // único

        [Required, Url, MaxLength(2048)]
        public string OriginalUrl { get; set; } = default!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public long ClickCount { get; set; } = 0;

        public DateTime= LastAccessedAt { get; set; }
    }
}