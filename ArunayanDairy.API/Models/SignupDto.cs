using System.ComponentModel.DataAnnotations;

namespace ArunayanDairy.API.Models;

/// <summary>
/// Signup request DTO matching POST /api/auth/signup
/// </summary>
public class SignupDto
{
    [Required]
    [MinLength(2)]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;
}
