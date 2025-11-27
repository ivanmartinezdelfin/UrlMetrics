using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace UrlMetrics.Api.Models
{
    //Índice único para que no se repitan shortcodes
    [Index(nameof(Shortcode), isUnique = true)]
    public class Links
    {
        public int Id { get; set; }

        [Required, MaxLength(12)]
        public string ShortCode { get; set; } = default!; // único

       
       [Required, Url, MaxLength(2048)]
        public string OriginalUrl { get; set; } = default!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public long ClickCount { get; set; } 

        public DateTime?  LastAccessedAt { get; set; }

        // Para desactivar un link sin borrarlo
        public bool IsActive { get; set; } = true;
    }
}