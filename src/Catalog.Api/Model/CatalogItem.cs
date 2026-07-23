namespace Catalog.Api.Model;

public class CatalogItem
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public string Description { get; set; } = "";

    public decimal Price { get; set; }

    public string Brand { get; set; } = "";

    public string Type { get; set; } = "";

    public string? PictureUrl { get; set; }

    public int AvailableStock { get; set; }
}
