using Microsoft.EntityFrameworkCore;
using UrlMetrics.Api.Models;

namespace UrlMetrics.Api.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Links> Links => Set<Links>();

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base (options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Config extra para customizar más adelante
            modelBuilder.Entity<Links>(entity =>
            {
                entity.Property(l => l.ShortCode)
                    .IsRequired()
                    .HasMaxLength(12);

                entity.Property(l => l.OriginalUrl)
                    .IsRequired()
                    .HasMaxLength(2048);

                entity.HasIndex(l => l.ShortCode)
                    .IsUnique();
                    

            });
        }
    }    
}
