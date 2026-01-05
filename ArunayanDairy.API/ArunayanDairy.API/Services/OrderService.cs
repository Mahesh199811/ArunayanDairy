using ArunayanDairy.API.DTOs.Orders;
using ArunayanDairy.API.Models;
using ArunayanDairy.API.Repositories.Interfaces;
using ArunayanDairy.API.Services.Interfaces;

namespace ArunayanDairy.API.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;

    public OrderService(IOrderRepository orderRepository, IProductRepository productRepository)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
    }

    public async Task<OrderResponseDto?> GetByIdAsync(string id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null) return null;

        return MapToResponseDto(order);
    }

    public async Task<IEnumerable<OrderResponseDto>> GetAllAsync()
    {
        var orders = await _orderRepository.GetAllAsync();
        return orders.Select(MapToResponseDto);
    }

    public async Task<IEnumerable<OrderResponseDto>> GetByUserIdAsync(string userId)
    {
        var orders = await _orderRepository.GetByUserIdAsync(userId);
        return orders.Select(MapToResponseDto);
    }

    public async Task<OrderResponseDto> CreateAsync(string userId, OrderCreateDto orderDto)
    {
        var orderItems = new List<OrderItem>();
        decimal totalAmount = 0;

        foreach (var item in orderDto.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with id {item.ProductId} not found");
            }

            if (product.StockQuantity < item.Quantity)
            {
                throw new InvalidOperationException($"Insufficient stock for product {product.Name}");
            }

            var subtotal = product.Price * item.Quantity;
            orderItems.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Quantity = item.Quantity,
                Price = product.Price,
                Subtotal = subtotal
            });

            totalAmount += subtotal;

            // Update product stock
            product.StockQuantity -= item.Quantity;
            await _productRepository.UpdateAsync(product);
        }

        var order = new Order
        {
            UserId = userId,
            Items = orderItems,
            TotalAmount = totalAmount,
            DeliveryAddress = orderDto.DeliveryAddress,
            PaymentMethod = orderDto.PaymentMethod,
            DeliveryDate = orderDto.DeliveryDate,
            Status = "Pending"
        };

        await _orderRepository.CreateAsync(order);
        return MapToResponseDto(order);
    }

    public async Task<OrderResponseDto> UpdateStatusAsync(string id, string status)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
        {
            throw new KeyNotFoundException($"Order with id {id} not found");
        }

        order.Status = status;
        await _orderRepository.UpdateAsync(order);
        return MapToResponseDto(order);
    }

    public async Task DeleteAsync(string id)
    {
        await _orderRepository.DeleteAsync(id);
    }

    private static OrderResponseDto MapToResponseDto(Order order)
    {
        return new OrderResponseDto
        {
            Id = order.Id,
            UserId = order.UserId,
            Items = order.Items,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            DeliveryAddress = order.DeliveryAddress,
            PaymentMethod = order.PaymentMethod,
            OrderDate = order.OrderDate,
            DeliveryDate = order.DeliveryDate
        };
    }
}
