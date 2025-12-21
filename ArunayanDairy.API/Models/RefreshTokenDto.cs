using System.ComponentModel.DataAnnotations;

namespace ArunayanDairy.API.Models;

/// <summary>
/// Refresh token request DTO matching POST /api/auth/refresh
/// </summary>
public class RefreshTokenDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
