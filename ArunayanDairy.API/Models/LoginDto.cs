using System.ComponentModel.DataAnnotations;

namespace ArunayanDairy.API.Models;

/// <summary>
/// Login request DTO matching POST /api/auth/login
/// </summary>
public class LoginDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;
}
