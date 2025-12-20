using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UrlMetrics.Api.Data;
using UrlMetrics.Api.Dtos;
using UrlMetrics.Api.Models;
using UrlMetrics.Api.Services;

namespace UrlMetrics.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LinksController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly IShortCodeService _shortCodeService;
        private readonly ILogger<LinksController> _logger;

        public LinksController(
            AppDbContext dbContext,
            IShortCodeService shortCodeService,
            ILogger<LinksController> logger)
        {
            _dbContext = dbContext;
            _shortCodeService = shortCodeService;
            _logger = logger;
        }

        // POST api/links
        [HttpPost]
        public async Task<ActionResult<LinkResponse>> CreateAsync(
            [FromBody] CreateLinkRequest request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var shortCode = await _shortCodeService.GenerateUniqueCodeAsync(
                request.CustomCode,
                cancellationToken);

            var link = new Links
            {
                ShortCode = shortCode,
                OriginalUrl = request.Url
            };

            _dbContext.Links.Add(link);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var response = ToResponse(link);

            _logger.LogInformation("Created short url {ShortCode} for {Url}", shortCode, request.Url);

            return CreatedAtAction(nameof(GetByIdAsync), new { id = link.Id }, response);
        }

        // GET api/links?page=1&pageSize=20&search=google
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LinkResponse>>> GetAllAsync(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            CancellationToken cancellationToken = default)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 100) pageSize = 20;

            var query = _dbContext.Links.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(l =>
                    l.ShortCode.Contains(search) ||
                    l.OriginalUrl.Contains(search));
            }

            var total = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(l => l.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            Response.Headers["X-Total-Count"] = total.ToString();

            return Ok(items.Select(ToResponse));
        }

        // GET api/links/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<LinkResponse>> GetByIdAsync(
            int id,
            CancellationToken cancellationToken)
        {
            var link = await _dbContext.Links.FindAsync(new object?[] { id }, cancellationToken);

            if (link is null)
            {
                return NotFound();
            }

            return Ok(ToResponse(link));
        }

        // PUT api/links/5
        [HttpPut("{id:int}")]
        public async Task<ActionResult<LinkResponse>> UpdateAsync(
            int id,
            [FromBody] UpdateLinkRequest request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var link = await _dbContext.Links.FindAsync(new object?[] { id }, cancellationToken);
            if (link is null)
            {
                return NotFound();
            }

            link.OriginalUrl = request.Url;
            if (request.IsActive.HasValue)
            {
                link.IsActive = request.IsActive.Value;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(ToResponse(link));
        }

        // DELETE api/links/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAsync(
            int id,
            CancellationToken cancellationToken)
        {
            var link = await _dbContext.Links.FindAsync(new object?[] { id }, cancellationToken);
            if (link is null)
            {
                return NotFound();
            }

            _dbContext.Links.Remove(link);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return NoContent();
        }

        // GET api/links/resolve/abc123  -> redirige y suma click
        [HttpGet("resolve/{code}")]
        public async Task<IActionResult> ResolveAsync(
            string code,
            CancellationToken cancellationToken)
        {
            var link = await _dbContext.Links
                .FirstOrDefaultAsync(l => l.ShortCode == code, cancellationToken);

            if (link is null || !link.IsActive)
            {
                return NotFound();
            }

            link.ClickCount++;
            link.LastAccessedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Redirect(link.OriginalUrl);
        }

        // GET api/links/5/stats
        [HttpGet("{id:int}/stats")]
        public async Task<ActionResult<object>> GetStatsAsync(
            int id,
            CancellationToken cancellationToken)
        {
            var link = await _dbContext.Links.FindAsync(new object?[] { id }, cancellationToken);
            if (link is null)
            {
                return NotFound();
            }

            return Ok(new
            {
                link.Id,
                link.ShortCode,
                link.OriginalUrl,
                link.CreatedAt,
                link.ClickCount,
                link.LastAccessedAt,
                link.IsActive
            });
        }

        private static LinkResponse ToResponse(Links link) =>
            new()
            {
                Id = link.Id,
                ShortCode = link.ShortCode,
                OriginalUrl = link.OriginalUrl,
                CreatedAt = link.CreatedAt,
                ClickCount = link.ClickCount,
                LastAccessedAt = link.LastAccessedAt,
                IsActive = link.IsActive
            };
    }
}