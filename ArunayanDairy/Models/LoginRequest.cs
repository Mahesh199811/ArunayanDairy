namespace ArunayanDairy.Models;

/// <summary>
/// Request payload for POST /api/auth/login
/// </summary>
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
