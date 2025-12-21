namespace ArunayanDairy.Models;

/// <summary>
/// Request payload for POST /api/auth/refresh
/// </summary>
public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}
