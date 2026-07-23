using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting;

/// <summary>
/// Aspire hosting integration for the eShop Catalog API.
/// This is the contract the catalog team publishes for orchestrating teams:
/// they get the published container image plus well-known knobs (seed data)
/// without needing to know anything about the catalog internals.
/// </summary>
public static class CatalogApiExtensions
{
    public const string DefaultImage = "ghcr.io/lpichet/eshop-catalog-api";
    public const string DefaultTag = "latest";

    private const string ConnectionName = "catalogdb";
    private const string SeedFileEnvVar = "Catalog__SeedFile";
    private const string ContainerSeedPath = "/seed/catalog.json";

    /// <summary>
    /// Adds the Catalog API container (published to GitHub Container Registry by the catalog team)
    /// to the application model, wired to the given PostgreSQL database.
    /// </summary>
    public static IResourceBuilder<ContainerResource> AddCatalogApi(
        this IDistributedApplicationBuilder builder,
        string name,
        IResourceBuilder<IResourceWithConnectionString> database,
        string? tag = null)
    {
        return builder.AddContainer(name, DefaultImage, tag ?? DefaultTag)
            .WithHttpEndpoint(targetPort: 8080)
            .WithReference(database, ConnectionName)
            .WaitFor(database)
            .WithHttpHealthCheck("/health");
    }

    /// <summary>
    /// Replaces the catalog's built-in seed data with a JSON file supplied by the consumer.
    /// The path is resolved relative to the AppHost directory. Works for the container
    /// resource added by <see cref="AddCatalogApi"/> (the file is bind-mounted into the container).
    /// </summary>
    public static IResourceBuilder<ContainerResource> WithSeedData(
        this IResourceBuilder<ContainerResource> builder,
        string seedFilePath)
    {
        var fullPath = Path.GetFullPath(seedFilePath, builder.ApplicationBuilder.AppHostDirectory);
        return builder
            .WithBindMount(fullPath, ContainerSeedPath, isReadOnly: true)
            .WithEnvironment(SeedFileEnvVar, ContainerSeedPath);
    }

    /// <summary>
    /// Same as the container overload, but for running the Catalog API from source
    /// (a <see cref="ProjectResource"/>) — used by the catalog team's own AppHost and tests.
    /// </summary>
    public static IResourceBuilder<ProjectResource> WithSeedData(
        this IResourceBuilder<ProjectResource> builder,
        string seedFilePath)
    {
        var fullPath = Path.GetFullPath(seedFilePath, builder.ApplicationBuilder.AppHostDirectory);
        return builder.WithEnvironment(SeedFileEnvVar, fullPath);
    }
}
