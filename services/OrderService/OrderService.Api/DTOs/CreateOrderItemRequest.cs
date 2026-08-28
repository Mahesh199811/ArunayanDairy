namespace OrderService.Api.DTOs;

public class CreateOrderItemRequest
{
    public Guid ProductId { get; set; }

    public decimal Quantity { get; set; }
}
