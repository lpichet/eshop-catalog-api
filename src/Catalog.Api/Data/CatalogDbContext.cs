using Catalog.Api.Model;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Api.Data;

public class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : DbContext(options)
{
    public DbSet<CatalogItem> Items => Set<CatalogItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CatalogItem>(entity =>
        {
            entity.ToTable("catalog_items");
            entity.Property(i => i.Name).HasMaxLength(100);
            entity.Property(i => i.Brand).HasMaxLength(100);
            entity.Property(i => i.Type).HasMaxLength(100);
            entity.Property(i => i.Price).HasColumnType("numeric(18,2)");
        });
    }
}
