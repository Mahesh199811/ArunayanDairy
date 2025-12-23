namespace ArunayanDairy.API.Models;

/// <summary>
/// Dairy product entity
/// </summary>
public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string SKU { get; set; } = string.Empty; // Stock Keeping Unit
    public ProductUnit Unit { get; set; }
    public decimal BasePrice { get; set; } // Price per unit
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public decimal MinOrderQuantity { get; set; } = 1;
    public decimal? MaxOrderQuantity { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Category Category { get; set; } = null!;
    public ICollection<ProductAvailability> Availabilities { get; set; } = new List<ProductAvailability>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
