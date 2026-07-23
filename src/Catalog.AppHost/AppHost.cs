var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres");
var catalogDb = postgres.AddDatabase("catalogdb");

var catalogApi = builder.AddProject<Projects.Catalog_Api>("catalog-api")
    .WithReference(catalogDb)
    .WaitFor(catalogDb)
    .WithHttpHealthCheck("/health");

// The catalog team's own local run uses the built-in seed by default.
// Tests (and the CLI) can swap it:
//   dotnet run --project src/Catalog.AppHost -- --Catalog:SeedFile=/path/to/seed.json
if (builder.Configuration["Catalog:SeedFile"] is { Length: > 0 } seedFile)
{
    catalogApi.WithSeedData(seedFile);
}

builder.Build().Run();
