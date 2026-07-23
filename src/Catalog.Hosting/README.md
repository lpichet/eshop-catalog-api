# EShop.Catalog.Hosting

Aspire hosting integration for the eShop demo **Catalog API**, published by the catalog team.

```csharp
var postgres = builder.AddPostgres("postgres");
var catalogDb = postgres.AddDatabase("catalogdb");

var catalogApi = builder.AddCatalogApi("catalog-api", catalogDb)
    .WithSeedData("./seed/my-catalog.json"); // optional: replace the built-in seed
```

- `AddCatalogApi(name, database, tag?)` adds the `ghcr.io/lpichet/eshop-catalog-api` container, wired to your PostgreSQL database, with a health check on `/health`.
- `WithSeedData(path)` replaces the catalog's built-in seed data with your own JSON file (bind-mounted into the container). The JSON format is an array of items:

```json
[
  { "name": "My product", "description": "…", "price": 9.99, "brand": "Acme", "type": "Gadgets", "availableStock": 10 }
]
```
