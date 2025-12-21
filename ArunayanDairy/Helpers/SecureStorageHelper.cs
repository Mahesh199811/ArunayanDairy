using System.Text.Json;
using ArunayanDairy.Models;

namespace ArunayanDairy.Helpers;

/// <summary>
/// Helper class for securely storing and retrieving authentication tokens and user data.
/// Uses MAUI SecureStorage for platform-specific secure storage (Keychain on iOS, Keystore on Android).
/// </summary>
public class SecureStorageHelper
{
    private const string AccessTokenKey = "access_token";
    private const string RefreshTokenKey = "refresh_token";
    private const string TokenExpiresInKey = "token_expires_in";
    private const string UserKey = "user_data";

    /// <summary>
    /// Save authentication tokens to secure storage.
    /// </summary>
    public async Task SaveTokensAsync(AuthTokens tokens)
    {
        try
        {
            await SecureStorage.Default.SetAsync(AccessTokenKey, tokens.AccessToken);
            await SecureStorage.Default.SetAsync(RefreshTokenKey, tokens.RefreshToken);
            await SecureStorage.Default.SetAsync(TokenExpiresInKey, tokens.ExpiresIn.ToString());
        }
        catch (Exception ex)
        {
            // Fallback to Preferences for iOS Simulator or when Keychain is not available
            Console.WriteLine($"SecureStorage failed, using Preferences: {ex.Message}");
            Preferences.Default.Set(AccessTokenKey, tokens.AccessToken);
            Preferences.Default.Set(RefreshTokenKey, tokens.RefreshToken);
            Preferences.Default.Set(TokenExpiresInKey, tokens.ExpiresIn.ToString());
        }
    }

    /// <summary>
    /// Get access token from secure storage.
    /// </summary>
    public async Task<string?> GetAccessTokenAsync()
    {
        try
        {
            return await SecureStorage.Default.GetAsync(AccessTokenKey);
        }
        catch
        {
            return Preferences.Default.Get(AccessTokenKey, string.Empty);
        }
    }

    /// <summary>
    /// Get refresh token from secure storage.
    /// </summary>
    public async Task<string?> GetRefreshTokenAsync()
    {
        try
        {
            return await SecureStorage.Default.GetAsync(RefreshTokenKey);
        }
        catch
        {
            return Preferences.Default.Get(RefreshTokenKey, string.Empty);
        }
    }

    /// <summary>
    /// Get token expiration time (in seconds).
    /// </summary>
    public async Task<int?> GetTokenExpiresInAsync()
    {
        try
        {
            var value = await SecureStorage.Default.GetAsync(TokenExpiresInKey);
            return int.TryParse(value, out var expiresIn) ? expiresIn : null;
        }
        catch
        {
            var value = Preferences.Default.Get(TokenExpiresInKey, string.Empty);
            return int.TryParse(value, out var expiresIn) ? expiresIn : null;
        }
    }

    /// <summary>
    /// Save user data to secure storage.
    /// </summary>
    public async Task SaveUserAsync(User user)
    {
        var json = JsonSerializer.Serialize(user);
        try
        {
            await SecureStorage.Default.SetAsync(UserKey, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SecureStorage failed, using Preferences: {ex.Message}");
            Preferences.Default.Set(UserKey, json);
        }
    }

    /// <summary>
    /// Get user data from secure storage.
    /// </summary>
    public async Task<User?> GetUserAsync()
    {
        string? json = null;
        try
        {
            json = await SecureStorage.Default.GetAsync(UserKey);
        }
        catch
        {
            json = Preferences.Default.Get(UserKey, string.Empty);
        }
        
        if (string.IsNullOrEmpty(json))
            return null;

        return JsonSerializer.Deserialize<User>(json);
    }

    /// <summary>
    /// Clear all stored authentication data.
    /// </summary>
    public async Task ClearAllAsync()
    {
        try
        {
            SecureStorage.Default.Remove(AccessTokenKey);
            SecureStorage.Default.Remove(RefreshTokenKey);
            SecureStorage.Default.Remove(TokenExpiresInKey);
            SecureStorage.Default.Remove(UserKey);
        }
        catch
        {
            Preferences.Default.Remove(AccessTokenKey);
            Preferences.Default.Remove(RefreshTokenKey);
            Preferences.Default.Remove(TokenExpiresInKey);
            Preferences.Default.Remove(UserKey);
        }
        await Task.CompletedTask;
    }

    /// <summary>
    /// Check if tokens exist in storage.
    /// </summary>
    public async Task<bool> HasTokensAsync()
    {
        var accessToken = await GetAccessTokenAsync();
        var refreshToken = await GetRefreshTokenAsync();
        return !string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(refreshToken);
    }
}
