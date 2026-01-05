using System.ComponentModel.DataAnnotations;

namespace ArunayanDairy.API.DTOs.Orders;

public class OrderCreateDto
{
    [Required]
    public List<OrderItemDto> Items { get; set; } = new();

    [Required]
    public string DeliveryAddress { get; set; } = string.Empty;

    [Required]
    public string PaymentMethod { get; set; } = string.Empty;

    public DateTime? DeliveryDate { get; set; }
}

public class OrderItemDto
{
    [Required]
    public string ProductId { get; set; } = string.Empty;

    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}
