namespace ArunayanDairy.API.Models;

/// <summary>
/// Individual item within an order
/// </summary>
public class OrderItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; } // Price at time of order
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Order Order { get; set; } = null!;
    public Product Product { get; set; } = null!;

    /// <summary>
    /// Calculated subtotal for this item
    /// </summary>
    public decimal Subtotal => Quantity * UnitPrice;
}
