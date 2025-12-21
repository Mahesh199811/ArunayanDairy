namespace ArunayanDairy.Models;

/// <summary>
/// Response from POST /api/auth/login
/// </summary>
public class LoginResponse
{
    public User User { get; set; } = new();
    public AuthTokens Tokens { get; set; } = new();
}

public class User
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class AuthTokens
{
    public string AccessToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
}
