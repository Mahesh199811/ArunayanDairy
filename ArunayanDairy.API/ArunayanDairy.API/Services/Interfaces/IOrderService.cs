using ArunayanDairy.API.DTOs.Orders;

namespace ArunayanDairy.API.Services.Interfaces;

public interface IOrderService
{
    Task<OrderResponseDto?> GetByIdAsync(string id);
    Task<IEnumerable<OrderResponseDto>> GetAllAsync();
    Task<IEnumerable<OrderResponseDto>> GetByUserIdAsync(string userId);
    Task<OrderResponseDto> CreateAsync(string userId, OrderCreateDto orderDto);
    Task<OrderResponseDto> UpdateStatusAsync(string id, string status);
    Task DeleteAsync(string id);
}
