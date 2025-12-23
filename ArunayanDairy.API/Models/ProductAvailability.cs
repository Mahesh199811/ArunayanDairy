namespace ArunayanDairy.API.Models;

/// <summary>
/// Date-specific product availability and pricing
/// </summary>
public class ProductAvailability
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProductId { get; set; }
    public DateTime AvailableDate { get; set; } // Date only (time set to midnight UTC)
    public decimal StockQuantity { get; set; }
    public decimal? PriceOverride { get; set; } // Optional price override for this date
    public bool IsAvailable { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public Product Product { get; set; } = null!;

    /// <summary>
    /// Gets the effective price for this availability (override or base price)
    /// </summary>
    public decimal GetEffectivePrice()
    {
        return PriceOverride ?? Product?.BasePrice ?? 0;
    }
}
