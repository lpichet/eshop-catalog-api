using Catalog.Api.Apis;
using Catalog.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<CatalogDbContext>("catalogdb");
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapOpenApi();
app.MapCatalogApi();

await CatalogSeeder.SeedAsync(app.Services, app.Lifetime.ApplicationStopping);

await app.RunAsync();
