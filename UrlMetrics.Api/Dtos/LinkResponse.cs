using System;

namespace UrlMetrics.Api.Dtos
{
    // DTO que devuelves al cliente (lo que ve el front)
    public class LinkResponse
    {
        public int Id { get; init; }
        public string ShortCode { get; init; } = default!;
        public string OriginalUrl { get; init; } = default!;
        public DateTime CreatedAt { get; init; } = default!;
        public long ClickCount { get; init; } = default!;
        public DateTime? LastAccessedAt { get; init; }
        public bool IsActive { get; init; }
    }
}