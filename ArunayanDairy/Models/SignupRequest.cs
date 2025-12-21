namespace ArunayanDairy.Models;

/// <summary>
/// Request payload for POST /api/auth/signup
/// </summary>
public class SignupRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
