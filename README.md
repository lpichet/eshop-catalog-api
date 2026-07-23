# eShop Catalog API

The **catalog team's** repository in the [eShop Aspire multi-team demo](https://github.com/lpichet/eshop). Owns the product catalog: an ASP.NET Core (.NET 10) minimal API backed by PostgreSQL, orchestrated with [Aspire 13.4](https://aspire.dev).

This repo is deliberately independent: the catalog team builds, tests, versions and ships on its own cadence. What it publishes for other teams:

| Artifact | Where | What consumers get |
|---|---|---|
| Container image | `ghcr.io/lpichet/eshop-catalog-api` | The runnable API (multi-arch: amd64 + arm64) |
| NuGet package | `EShop.Catalog.Hosting` on GitHub Packages | Aspire hosting integration: `AddCatalogApi()` + `WithSeedData()` |

## Run it locally (catalog team inner loop)

Prereqs: .NET 10 SDK, Docker (or Podman), [Aspire CLI](https://aspire.dev).

```bash
aspire run --project src/Catalog.AppHost
# or: dotnet run --project src/Catalog.AppHost
```

This starts PostgreSQL in a container, runs the API **from source**, seeds it with the built-in [`Seed/catalog.json`](src/Catalog.Api/Seed/catalog.json), and opens the Aspire dashboard.

Endpoints:

- `GET /api/catalog/items?pageIndex=0&pageSize=20&brand=Daybird`
- `GET /api/catalog/items/{id}`
- `GET /api/catalog/brands`
- `GET /health`, `GET /openapi/v1.json`

## Data seeding

On startup the API creates the schema and, if the database is empty, seeds it from a JSON file:

1. By default: the `Seed/catalog.json` file shipped inside the image.
2. If the `Catalog:SeedFile` configuration key is set: that file instead.

Orchestrators never set the raw env var themselves — they use the extension method from `EShop.Catalog.Hosting`:

```csharp
builder.AddCatalogApi("catalog-api", catalogDb)
       .WithSeedData("./seed/my-own-catalog.json");
```

`WithSeedData` bind-mounts the file into the container and points `Catalog:SeedFile` at it. There is a `ProjectResource` overload too, so the same call works when running from source (this repo's own AppHost and tests use it).

## Tests

`tests/Catalog.IntegrationTests` uses **Aspire.Hosting.Testing**: each test boots the real AppHost (real PostgreSQL container, real API) and calls the HTTP API. One test verifies the built-in seed; another replaces the seed with a test-specific JSON file and verifies the replacement won.

```bash
dotnet test
```

## CI/CD

[.github/workflows/ci.yml](.github/workflows/ci.yml): on every push to `main`, after the integration tests pass, CI publishes

- the container image to GHCR (`latest`, `1.0.<run>`, `sha-<commit>` tags) using .NET SDK container publish (no Dockerfile), and
- the `EShop.Catalog.Hosting` package to GitHub Packages with the matching version.
