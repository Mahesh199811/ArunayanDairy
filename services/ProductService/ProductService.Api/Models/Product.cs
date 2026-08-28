namespace ProductService.Api.Models;

public class Product
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string Unit { get; set; } = string.Empty;

    public decimal AvailableQuantity { get; set; }

    public DateTime AvailableDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}