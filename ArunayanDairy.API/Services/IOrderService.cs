using ArunayanDairy.API.Models;

namespace ArunayanDairy.API.Services;

/// <summary>
/// Service interface for order management
/// </summary>
public interface IOrderService
{
    Task<OrderDto> CreateOrderAsync(Guid customerId, CreateOrderDto dto);
    Task<OrderDto?> GetOrderByIdAsync(Guid orderId, Guid? customerId = null);
    Task<IEnumerable<OrderDto>> GetCustomerOrdersAsync(Guid customerId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<IEnumerable<OrderDto>> GetAllOrdersAsync(DateTime? fromDate = null, DateTime? toDate = null, OrderStatus? status = null);
    Task<OrderDto?> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus);
    Task<bool> CancelOrderAsync(Guid orderId, Guid customerId);
    Task<OrderSummaryDto> GetOrderSummaryAsync(DateTime? fromDate = null, DateTime? toDate = null);
}
