using Catalog.Api.Data;
using Catalog.Api.Model;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Api.Apis;

public static class CatalogApi
{
    public static IEndpointRouteBuilder MapCatalogApi(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api/catalog");

        api.MapGet("/items", GetItems);
        api.MapGet("/items/{id:int}", GetItemById);
        api.MapGet("/brands", GetBrands);

        return app;
    }

    private static async Task<Ok<PaginatedItems<CatalogItem>>> GetItems(
        CatalogDbContext db,
        int pageIndex = 0,
        int pageSize = 20,
        string? brand = null)
    {
        var query = db.Items.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(brand))
        {
            query = query.Where(i => i.Brand == brand);
        }

        var count = await query.CountAsync();
        var data = await query
            .OrderBy(i => i.Name)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return TypedResults.Ok(new PaginatedItems<CatalogItem>(pageIndex, pageSize, count, data));
    }

    private static async Task<Results<Ok<CatalogItem>, NotFound>> GetItemById(CatalogDbContext db, int id)
    {
        var item = await db.Items.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
        return item is null ? TypedResults.NotFound() : TypedResults.Ok(item);
    }

    private static async Task<Ok<List<string>>> GetBrands(CatalogDbContext db)
    {
        var brands = await db.Items.AsNoTracking()
            .Select(i => i.Brand)
            .Distinct()
            .OrderBy(b => b)
            .ToListAsync();
        return TypedResults.Ok(brands);
    }
}

public record PaginatedItems<T>(int PageIndex, int PageSize, int Count, List<T> Data);
