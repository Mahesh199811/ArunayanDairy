namespace ArunayanDairy.API.Models;

/// <summary>
/// Order status lifecycle
/// </summary>
public enum OrderStatus
{
    Pending,         // Order placed, awaiting confirmation
    Confirmed,       // Order confirmed by admin
    Processing,      // Being prepared
    OutForDelivery,  // Out for delivery
    Delivered,       // Successfully delivered
    Cancelled        // Cancelled by customer or admin
}
