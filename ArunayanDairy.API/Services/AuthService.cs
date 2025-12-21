using Microsoft.EntityFrameworkCore;
using ArunayanDairy.API.Data;
using ArunayanDairy.API.Models;
using ArunayanDairy.API.Security;

namespace ArunayanDairy.API.Services;

/// <summary>
/// Authentication service implementation with SQLite database.
/// </summary>
public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly JwtTokenGenerator _tokenGenerator;

    public AuthService(ApplicationDbContext context, JwtTokenGenerator tokenGenerator)
    {
        _context = context;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<AuthResponse?> LoginAsync(LoginDto loginDto)
    {
        // Find user by email
        var user = await GetUserByEmailAsync(loginDto.Email);
        if (user == null)
            return null;

        // Verify password
        if (!PasswordHasher.Verify(loginDto.Password, user.PasswordHash))
            return null;

        // Generate tokens
        var accessToken = _tokenGenerator.GenerateAccessToken(user);
        var refreshToken = _tokenGenerator.GenerateRefreshToken();

        // Store refresh token
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_tokenGenerator.GetRefreshTokenExpiryDays());
        user.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            User = MapToUserDto(user),
            Tokens = new TokensDto
            {
                AccessToken = accessToken,
                ExpiresIn = _tokenGenerator.GetAccessTokenExpirySeconds(),
                RefreshToken = refreshToken
            }
        };
    }

    public async Task<AuthResponse?> SignupAsync(SignupDto signupDto)
    {
        // Check if user already exists
        var existingUser = await GetUserByEmailAsync(signupDto.Email);
        if (existingUser != null)
            return null; // Email already taken

        // Create new user
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = signupDto.Name,
            Email = signupDto.Email.ToLower(),
            PasswordHash = PasswordHasher.Hash(signupDto.Password),
            Role = "user",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Generate tokens
        var accessToken = _tokenGenerator.GenerateAccessToken(user);
        var refreshToken = _tokenGenerator.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_tokenGenerator.GetRefreshTokenExpiryDays());

        // Add to database
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            User = MapToUserDto(user),
            Tokens = new TokensDto
            {
                AccessToken = accessToken,
                ExpiresIn = _tokenGenerator.GetAccessTokenExpirySeconds(),
                RefreshToken = refreshToken
            }
        };
    }

    public async Task<TokensDto?> RefreshTokenAsync(string refreshToken)
    {
        // Find user with matching refresh token
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
        
        if (user == null)
            return null;

        // Check if refresh token is expired
        if (user.RefreshTokenExpiryTime == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            return null;

        // Generate new tokens
        var newAccessToken = _tokenGenerator.GenerateAccessToken(user);
        var newRefreshToken = _tokenGenerator.GenerateRefreshToken();

        // Update refresh token
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_tokenGenerator.GetRefreshTokenExpiryDays());
        user.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();

        return new TokensDto
        {
            AccessToken = newAccessToken,
            ExpiresIn = _tokenGenerator.GetAccessTokenExpirySeconds(),
            RefreshToken = newRefreshToken
        };
    }

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        return await _context.Users.FindAsync(userId);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }

    private static UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id.ToString(),
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}
