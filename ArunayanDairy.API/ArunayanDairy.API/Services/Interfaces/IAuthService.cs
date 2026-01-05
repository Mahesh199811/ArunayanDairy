using ArunayanDairy.API.DTOs.Auth;

namespace ArunayanDairy.API.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    Task<AuthResponseDto> SignupAsync(SignupRequestDto request);
}
