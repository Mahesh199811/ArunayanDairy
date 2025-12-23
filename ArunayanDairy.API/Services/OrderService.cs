using Microsoft.EntityFrameworkCore;
using ArunayanDairy.API.Models;
using ArunayanDairy.API.Repositories;

namespace ArunayanDairy.API.Services;

/// <summary>
/// Service implementation for order management
/// </summary>
public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IUnitOfWork unitOfWork, ILogger<OrderService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<OrderDto> CreateOrderAsync(Guid customerId, CreateOrderDto dto)
    {
        // Validate customer exists
        var customer = await _unitOfWork.Users.GetByIdAsync(customerId);
        if (customer == null)
            throw new InvalidOperationException("Customer not found");

        // Validate delivery date is in the future
        if (dto.DeliveryDate.Date < DateTime.UtcNow.Date)
            throw new InvalidOperationException("Delivery date must be in the future");

        // Validate items
        if (dto.Items == null || !dto.Items.Any())
            throw new InvalidOperationException("Order must contain at least one item");

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var order = new Order
            {
                CustomerId = customerId,
                OrderNumber = await GenerateOrderNumberAsync(),
                OrderDate = DateTime.UtcNow,
                DeliveryDate = dto.DeliveryDate.Date,
                Status = OrderStatus.Pending,
                Notes = dto.Notes
            };

            // Process each item
            foreach (var itemDto in dto.Items)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(itemDto.ProductId);
                if (product == null)
                    throw new InvalidOperationException($"Product {itemDto.ProductId} not found");

                if (!product.IsActive)
                    throw new InvalidOperationException($"Product {product.Name} is not available");

                // Validate quantity
                if (itemDto.Quantity < product.MinOrderQuantity)
                    throw new InvalidOperationException($"Minimum order quantity for {product.Name} is {product.MinOrderQuantity} {product.Unit}");

                if (product.MaxOrderQuantity.HasValue && itemDto.Quantity > product.MaxOrderQuantity.Value)
                    throw new InvalidOperationException($"Maximum order quantity for {product.Name} is {product.MaxOrderQuantity} {product.Unit}");

                // Check availability for delivery date
                var availability = await _unitOfWork.ProductAvailabilities.FirstOrDefaultAsync(pa =>
                    pa.ProductId == itemDto.ProductId &&
                    pa.AvailableDate.Date == dto.DeliveryDate.Date &&
                    pa.IsAvailable);

                if (availability == null)
                    throw new InvalidOperationException($"Product {product.Name} is not available for {dto.DeliveryDate:yyyy-MM-dd}");

                if (availability.StockQuantity < itemDto.Quantity)
                    throw new InvalidOperationException($"Insufficient stock for {product.Name}. Available: {availability.StockQuantity} {product.Unit}");

                // Get effective price
                var unitPrice = availability.PriceOverride ?? product.BasePrice;

                // Create order item
                var orderItem = new OrderItem
                {
                    ProductId = itemDto.ProductId,
                    Quantity = itemDto.Quantity,
                    UnitPrice = unitPrice
                };

                order.OrderItems.Add(orderItem);

                // Reduce stock quantity
                availability.StockQuantity -= itemDto.Quantity;
                _unitOfWork.ProductAvailabilities.Update(availability);
            }

            // Calculate total
            order.CalculateTotalAmount();

            await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation("Order {OrderNumber} created for customer {CustomerId} with total {TotalAmount:C}",
                order.OrderNumber, customerId, order.TotalAmount);

            return await MapToOrderDto(order);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error creating order for customer {CustomerId}", customerId);
            throw;
        }
    }

    public async Task<OrderDto?> GetOrderByIdAsync(Guid orderId, Guid? customerId = null)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
        if (order == null)
            return null;

        // If customerId is provided, ensure order belongs to customer
        if (customerId.HasValue && order.CustomerId != customerId.Value)
            return null;

        return await MapToOrderDto(order);
    }

    public async Task<IEnumerable<OrderDto>> GetCustomerOrdersAsync(Guid customerId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var orders = await _unitOfWork.Orders.FindAsync(o =>
            o.CustomerId == customerId &&
            (!fromDate.HasValue || o.OrderDate.Date >= fromDate.Value.Date) &&
            (!toDate.HasValue || o.OrderDate.Date <= toDate.Value.Date));

        var orderList = orders.OrderByDescending(o => o.OrderDate).ToList();
        var orderDtos = new List<OrderDto>();

        foreach (var order in orderList)
        {
            orderDtos.Add(await MapToOrderDto(order));
        }

        return orderDtos;
    }

    public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync(DateTime? fromDate = null, DateTime? toDate = null, OrderStatus? status = null)
    {
        var orders = await _unitOfWork.Orders.FindAsync(o =>
            (!fromDate.HasValue || o.OrderDate.Date >= fromDate.Value.Date) &&
            (!toDate.HasValue || o.OrderDate.Date <= toDate.Value.Date) &&
            (!status.HasValue || o.Status == status.Value));

        var orderList = orders.OrderByDescending(o => o.OrderDate).ToList();
        var orderDtos = new List<OrderDto>();

        foreach (var order in orderList)
        {
            orderDtos.Add(await MapToOrderDto(order));
        }

        return orderDtos;
    }

    public async Task<OrderDto?> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
        if (order == null)
            return null;

        // Validate status transition
        if (!IsValidStatusTransition(order.Status, newStatus))
            throw new InvalidOperationException($"Cannot transition from {order.Status} to {newStatus}");

        order.Status = newStatus;
        order.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Orders.Update(order);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Order {OrderNumber} status updated to {Status}", order.OrderNumber, newStatus);

        return await MapToOrderDto(order);
    }

    public async Task<bool> CancelOrderAsync(Guid orderId, Guid customerId)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
        if (order == null || order.CustomerId != customerId)
            return false;

        // Only allow cancellation for Pending or Confirmed orders
        if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Confirmed)
            throw new InvalidOperationException($"Cannot cancel order with status {order.Status}");

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // Restore stock quantities
            var orderItems = await _unitOfWork.OrderItems.FindAsync(oi => oi.OrderId == orderId);
            foreach (var item in orderItems)
            {
                var availability = await _unitOfWork.ProductAvailabilities.FirstOrDefaultAsync(pa =>
                    pa.ProductId == item.ProductId &&
                    pa.AvailableDate.Date == order.DeliveryDate.Date);

                if (availability != null)
                {
                    availability.StockQuantity += item.Quantity;
                    _unitOfWork.ProductAvailabilities.Update(availability);
                }
            }

            order.Status = OrderStatus.Cancelled;
            order.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Orders.Update(order);
            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation("Order {OrderNumber} cancelled by customer {CustomerId}", order.OrderNumber, customerId);

            return true;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error cancelling order {OrderId}", orderId);
            throw;
        }
    }

    public async Task<OrderSummaryDto> GetOrderSummaryAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var orders = await _unitOfWork.Orders.FindAsync(o =>
            (!fromDate.HasValue || o.OrderDate.Date >= fromDate.Value.Date) &&
            (!toDate.HasValue || o.OrderDate.Date <= toDate.Value.Date));

        var orderList = orders.ToList();
        var today = DateTime.UtcNow.Date;
        var startOfMonth = new DateTime(today.Year, today.Month, 1);

        return new OrderSummaryDto
        {
            TotalOrders = orderList.Count,
            PendingOrders = orderList.Count(o => o.Status == OrderStatus.Pending),
            ConfirmedOrders = orderList.Count(o => o.Status == OrderStatus.Confirmed),
            ProcessingOrders = orderList.Count(o => o.Status == OrderStatus.Processing),
            DeliveredOrders = orderList.Count(o => o.Status == OrderStatus.Delivered),
            CancelledOrders = orderList.Count(o => o.Status == OrderStatus.Cancelled),
            TotalRevenue = orderList.Where(o => o.Status != OrderStatus.Cancelled).Sum(o => o.TotalAmount),
            TodayRevenue = orderList.Where(o => o.OrderDate.Date == today && o.Status != OrderStatus.Cancelled).Sum(o => o.TotalAmount),
            MonthRevenue = orderList.Where(o => o.OrderDate >= startOfMonth && o.Status != OrderStatus.Cancelled).Sum(o => o.TotalAmount)
        };
    }

    #region Helper Methods

    private async Task<string> GenerateOrderNumberAsync()
    {
        var today = DateTime.UtcNow;
        var prefix = $"ORD-{today:yyyyMMdd}";

        var todayOrders = await _unitOfWork.Orders.CountAsync(o => o.OrderNumber.StartsWith(prefix));
        var sequence = todayOrders + 1;

        return $"{prefix}-{sequence:D4}";
    }

    private static bool IsValidStatusTransition(OrderStatus currentStatus, OrderStatus newStatus)
    {
        // Define valid status transitions
        return currentStatus switch
        {
            OrderStatus.Pending => newStatus is OrderStatus.Confirmed or OrderStatus.Cancelled,
            OrderStatus.Confirmed => newStatus is OrderStatus.Processing or OrderStatus.Cancelled,
            OrderStatus.Processing => newStatus is OrderStatus.OutForDelivery or OrderStatus.Cancelled,
            OrderStatus.OutForDelivery => newStatus is OrderStatus.Delivered or OrderStatus.Cancelled,
            OrderStatus.Delivered => false, // Cannot change from delivered
            OrderStatus.Cancelled => false, // Cannot change from cancelled
            _ => false
        };
    }

    private async Task<OrderDto> MapToOrderDto(Order order)
    {
        // Load customer
        var customer = await _unitOfWork.Users.GetByIdAsync(order.CustomerId);

        // Load order items
        var orderItems = await _unitOfWork.OrderItems.FindAsync(oi => oi.OrderId == order.Id);
        var itemList = orderItems.ToList();

        // Load products for order items
        var productIds = itemList.Select(oi => oi.ProductId).Distinct();
        var products = await _unitOfWork.Products.FindAsync(p => productIds.Contains(p.Id));
        var productDict = products.ToDictionary(p => p.Id);

        var itemDtos = itemList.Select(item =>
        {
            var product = productDict.GetValueOrDefault(item.ProductId);
            return new OrderItemDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductName = product?.Name ?? "Unknown",
                ProductUnit = product?.Unit.ToString() ?? "Unknown",
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                Subtotal = item.Subtotal
            };
        }).ToList();

        return new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerId = order.CustomerId,
            CustomerName = customer?.Name ?? "Unknown",
            CustomerEmail = customer?.Email ?? "Unknown",
            OrderDate = order.OrderDate,
            DeliveryDate = order.DeliveryDate,
            Status = order.Status.ToString(),
            TotalAmount = order.TotalAmount,
            Notes = order.Notes,
            Items = itemDtos
        };
    }

    #endregion
}
