namespace ArunayanDairy.API.Models;

/// <summary>
/// Authentication response matching the API contract (login/signup response)
/// </summary>
public class AuthResponse
{
    public UserDto User { get; set; } = new();
    public TokensDto Tokens { get; set; } = new();
}

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class TokensDto
{
    public string AccessToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; } // Seconds
    public string RefreshToken { get; set; } = string.Empty;
}
