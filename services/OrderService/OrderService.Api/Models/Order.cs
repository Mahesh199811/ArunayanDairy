namespace OrderService.Api.Models;

public class Order
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public DateTime ScheduledDate { get; set; }

    public decimal TotalAmount { get; set; }

    public string Status { get; set; } = "Pending";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<OrderItem> Items { get; set; } = new();
}
