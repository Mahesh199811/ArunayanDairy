namespace OrderService.Api.DTOs;

public class CreateOrderRequest
{
    public Guid UserId { get; set; }

    public DateTime ScheduledDate { get; set; }

    public List<CreateOrderItemRequest> Items { get; set; } = new();
}
