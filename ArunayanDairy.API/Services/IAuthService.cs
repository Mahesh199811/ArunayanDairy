using ArunayanDairy.API.Models;

namespace ArunayanDairy.API.Services;

/// <summary>
/// Authentication service interface
/// </summary>
public interface IAuthService
{
    Task<AuthResponse?> LoginAsync(LoginDto loginDto);
    Task<AuthResponse?> SignupAsync(SignupDto signupDto);
    Task<TokensDto?> RefreshTokenAsync(string refreshToken);
    Task<User?> GetUserByIdAsync(Guid userId);
    Task<User?> GetUserByEmailAsync(string email);
}
