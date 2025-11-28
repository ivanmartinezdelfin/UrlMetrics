using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace UrlMetrics.Api.Middleware
{
    //Rate limiting muy simple por IP: X requests por minuto
    public class IpRateLimitMiddleware
    {
        private const int Limit = 60;           //requests
        private static readonly TimeSpan Window = TimeSpan.FromMinutes(1);

        private readonly RequestDelegate _next;
        private readonly ILogger<IpRateLimitMiddleware> _logger;
        private readonly IMemoryCache _cache;

        public IpRateLimitMiddleware(
            RequestDelegate next,
            ILogger<IpRateLimitMiddleware> logger,
            IMemoryCache cache)
        {
            _next = next;
            _logger = logger;
            _cache = cache;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknow";
            var cacheKey = $"rl:{ip}";

            var counter = _cache.GetOrCreate(cacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = Window;
                return 0;
            });

            if (counter >= Limit)
            {
                _logger.LogWarning("Rate limit exceeded for IP {Ip}", ip);
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "To many requests",
                    limit = Limit,
                    windowsSeconds = (int)Window.TotalSeconds
                });
                return;
            }

            _cache.Set(cacheKey, counter + 1);

            await _next(context);
        }
    }

    public static class IpRateLimitMiddlewareExtensions
    {
        public static IApplicationBuilder UseIpRateLimiting(this IApplicationBuilder app)
            => app.UseMiddleware<IpRateLimitMiddleware>();
    }
}