using System.Net.Http.Json;
using Aspire.Hosting;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Catalog.IntegrationTests;

public class CatalogApiTests
{
    private static readonly TimeSpan StartupTimeout = TimeSpan.FromMinutes(5);

    [Fact]
    public async Task DefaultSeed_IsServedByCatalogApi()
    {
        await using var app = await StartAppHostAsync();

        var client = app.CreateHttpClient("catalog-api");
        var page = await client.GetFromJsonAsync<CatalogPage>("/api/catalog/items?pageSize=50");

        Assert.NotNull(page);
        Assert.True(page.Count > 0, "expected the built-in seed to produce catalog items");
        Assert.Contains(page.Data, i => i.Name == "Adventurer GPS Watch");
    }

    [Fact]
    public async Task CustomSeed_ReplacesBuiltInSeed()
    {
        var seedFile = Path.Combine(Path.GetTempPath(), $"catalog-seed-{Guid.NewGuid():N}.json");
        await File.WriteAllTextAsync(seedFile,
            """
            [
              { "name": "Integration Test Widget", "description": "Only exists in tests", "price": 1.23, "brand": "TestBrand", "type": "TestType", "availableStock": 42 }
            ]
            """);

        try
        {
            await using var app = await StartAppHostAsync($"--Catalog:SeedFile={seedFile}");

            var client = app.CreateHttpClient("catalog-api");
            var page = await client.GetFromJsonAsync<CatalogPage>("/api/catalog/items?pageSize=50");

            Assert.NotNull(page);
            var item = Assert.Single(page.Data);
            Assert.Equal("Integration Test Widget", item.Name);
            Assert.Equal("TestBrand", item.Brand);
        }
        finally
        {
            File.Delete(seedFile);
        }
    }

    private static async Task<DistributedApplication> StartAppHostAsync(params string[] args)
    {
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Catalog_AppHost>(args);
        appHost.Services.ConfigureHttpClientDefaults(http => http.AddStandardResilienceHandler());

        var app = await appHost.BuildAsync();
        await app.StartAsync();
        await app.ResourceNotifications
            .WaitForResourceHealthyAsync("catalog-api")
            .WaitAsync(StartupTimeout);
        return app;
    }

    private sealed record CatalogPage(int PageIndex, int PageSize, int Count, List<CatalogItemDto> Data);

    private sealed record CatalogItemDto(
        int Id, string Name, string? Description, decimal Price, string Brand, string Type, int AvailableStock);
}
