namespace ProductWeb.Models;

/// <summary>
/// Model produk — mirror dari ProductCatalogAPI.
/// Properti harus cocok dengan JSON yang dikembalikan API.
/// </summary>
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Category { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
