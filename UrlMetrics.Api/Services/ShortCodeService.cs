using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UrlMetrics.Api.Data;

namespace UrlMetrics.Api.Services
{
    public interface IShortCodeService
    {
        Task<string> GenerateUniqueCodeAsync(
            string? preferredCode = null,
            CancellationToken cancellationToken = default);
    }

    public class ShortCodeService : IShortCodeService
    {
        private const string Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private const int DefaultLength = 8;

        private readonly AppDbContext _dbContext;

        public ShortCodeService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<string> GenerateUniqueCodeAsync(
            string? preferredCode = null,
            CancellationToken cancellationToken = default)
        {
            // Si el usuario manda un código preferido, intenta usarlo
            if (!string.IsNullOrWhiteSpace(preferredCode))
            {
                var normalized = preferredCode.Trim();

                var exists = await _dbContext.Links
                    .AnyAsync(l => l.ShortCode == normalized, cancellationToken);
                
                if (!exists)
                {
                    return normalized;
                }
                // Si ya existe, seguimos generando aleatorio
            }

            // Varios intentos hasta encontrar uno libre
            for (var attempt = 0; attempt < 10; attempt++)
            {
                var code = GenerateRandomCode(DefaultLength);

                var exists = await _dbContext.Links
                    .AnyAsync(l => l.ShortCode == code, cancellationToken);
                
                if (!exists)
                {
                    return code;
                }
            }
            throw new InvalidOperationException(
                "No fue posible generar un shortcode único después de varios intentos.");

        }
        private static string GenerateRandomCode(int length)
        {
            var bytes = RandomNumberGenerator.GetBytes(length);
            var chars = new char[length];

            for (var i=0; i< length; i++)
            {
                chars[i] = Alphabet[bytes[i] % Alphabet.Length];
            }

            return new string(chars);
        }
    }
}