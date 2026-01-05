using ArunayanDairy.API.DTOs.Users;

namespace ArunayanDairy.API.Services.Interfaces;

public interface IUserService
{
    Task<UserResponseDto?> GetByIdAsync(string id);
    Task<IEnumerable<UserResponseDto>> GetAllAsync();
    Task DeleteAsync(string id);
}
