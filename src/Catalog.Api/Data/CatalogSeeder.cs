using System.Text.Json;
using Catalog.Api.Model;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Api.Data;

/// <summary>
/// Seeds the catalog database on startup. The seed source is a JSON file:
/// the file shipped in the image (<c>Seed/catalog.json</c>) by default, or the file
/// pointed to by the <c>Catalog:SeedFile</c> configuration key. Orchestrators replace
/// the seed through the <c>WithSeedData()</c> extension in the EShop.Catalog.Hosting package,
/// which sets that key.
/// </summary>
public static class CatalogSeeder
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("CatalogSeeder");

        // The orchestrator waits for the database to be healthy before starting us,
        // but be resilient to slow container startups anyway.
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await db.Database.EnsureCreatedAsync(cancellationToken);
                break;
            }
            catch (Exception ex) when (attempt < 10)
            {
                logger.LogWarning(ex, "Database not ready (attempt {Attempt}/10), retrying in 2s...", attempt);
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }
        }

        if (await db.Items.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Catalog database already contains data, skipping seed.");
            return;
        }

        var seedFile = config["Catalog:SeedFile"];
        var source = string.IsNullOrWhiteSpace(seedFile)
            ? Path.Combine(AppContext.BaseDirectory, "Seed", "catalog.json")
            : seedFile;

        var json = await File.ReadAllTextAsync(source, cancellationToken);
        var items = JsonSerializer.Deserialize<List<CatalogItem>>(json, JsonOptions) ?? [];
        foreach (var item in items)
        {
            item.Id = 0; // let the database generate keys
        }

        db.Items.AddRange(items);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Seeded {Count} catalog items from {Source}.", items.Count, source);
    }
}
