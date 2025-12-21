using ArunayanDairy.Models;

namespace ArunayanDairy.Services;

/// <summary>
/// Authentication service interface for login, signup, and token refresh.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Login with email and password.
    /// </summary>
    Task<LoginResponse?> LoginAsync(LoginRequest request);

    /// <summary>
    /// Sign up a new user.
    /// </summary>
    Task<LoginResponse?> SignupAsync(SignupRequest request);

    /// <summary>
    /// Refresh access token using refresh token.
    /// </summary>
    Task<AuthTokens?> RefreshTokenAsync(string refreshToken);

    /// <summary>
    /// Check if the user is authenticated (valid access token in storage).
    /// </summary>
    Task<bool> IsAuthenticatedAsync();

    /// <summary>
    /// Logout and clear stored tokens.
    /// </summary>
    Task LogoutAsync();
}
