using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ArunayanDairy.API.Models;

namespace ArunayanDairy.API.Security;

/// <summary>
/// JWT token generation and validation
/// </summary>
public class JwtTokenGenerator
{
    private readonly IConfiguration _configuration;

    public JwtTokenGenerator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Generate access token (short-lived)
    /// </summary>
    public string GenerateAccessToken(User user)
    {
        var secret = _configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];
        var expiryMinutes = int.Parse(_configuration["Jwt:AccessTokenExpiryMinutes"] ?? "15");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Generate refresh token (long-lived, random)
    /// </summary>
    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    /// <summary>
    /// Get access token expiry in seconds
    /// </summary>
    public int GetAccessTokenExpirySeconds()
    {
        var expiryMinutes = int.Parse(_configuration["Jwt:AccessTokenExpiryMinutes"] ?? "15");
        return expiryMinutes * 60;
    }

    /// <summary>
    /// Get refresh token expiry in days
    /// </summary>
    public int GetRefreshTokenExpiryDays()
    {
        return int.Parse(_configuration["Jwt:RefreshTokenExpiryDays"] ?? "7");
    }

    /// <summary>
    /// Validate token and extract claims
    /// </summary>
    public ClaimsPrincipal? ValidateToken(string token)
    {
        var secret = _configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];

        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = false, // We'll check expiry separately
            ClockSkew = TimeSpan.Zero
        };

        try
        {
            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            return principal;
        }
        catch
        {
            return null;
        }
    }
}
