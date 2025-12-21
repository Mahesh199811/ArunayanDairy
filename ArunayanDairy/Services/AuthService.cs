using System.Net.Http.Json;
using System.Text.Json;
using ArunayanDairy.Helpers;
using ArunayanDairy.Models;

namespace ArunayanDairy.Services;

/// <summary>
/// Implementation of IAuthService that communicates with the backend API.
/// </summary>
public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly SecureStorageHelper _storageHelper;
    
    // For Android Emulator use: http://10.0.2.2:5001
    // For iOS Simulator use: http://localhost:5001
    // For Physical Device use your machine's IP: http://192.168.1.XXX:5001
    // Note: Port 5001 used because macOS AirPlay uses port 5000
    private const string BaseUrl = "http://localhost:5001";

    public AuthService(HttpClient httpClient, SecureStorageHelper storageHelper)
    {
        _httpClient = httpClient;
        _storageHelper = storageHelper;
        _httpClient.BaseAddress = new Uri(BaseUrl);
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/auth/login", request);

            if (response.IsSuccessStatusCode)
            {
                var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (loginResponse != null)
                {
                    await _storageHelper.SaveTokensAsync(loginResponse.Tokens);
                    await _storageHelper.SaveUserAsync(loginResponse.User);
                }
                return loginResponse;
            }

            // Try to read error as JSON, fallback to status code message
            string errorMessage = "Login failed";
            try
            {
                var contentString = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrWhiteSpace(contentString))
                {
                    var error = await response.Content.ReadFromJsonAsync<ApiError>();
                    errorMessage = error?.Message ?? $"Login failed with status {response.StatusCode}";
                }
                else
                {
                    errorMessage = $"Login failed with status {response.StatusCode}";
                }
            }
            catch
            {
                errorMessage = $"Login failed with status {response.StatusCode}";
            }
            
            throw new Exception(errorMessage);
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Network error: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            if (ex.Message.StartsWith("Login failed") || ex.Message.StartsWith("Network error"))
                throw;
            throw new Exception($"Login error: {ex.Message}", ex);
        }
    }

    public async Task<LoginResponse?> SignupAsync(SignupRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/auth/signup", request);

            if (response.IsSuccessStatusCode)
            {
                var signupResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (signupResponse != null)
                {
                    await _storageHelper.SaveTokensAsync(signupResponse.Tokens);
                    await _storageHelper.SaveUserAsync(signupResponse.User);
                }
                return signupResponse;
            }

            // Try to read error as JSON, fallback to status code message
            string errorMessage = "Signup failed";
            try
            {
                var contentString = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrWhiteSpace(contentString))
                {
                    var error = await response.Content.ReadFromJsonAsync<ApiError>();
                    errorMessage = error?.Message ?? $"Signup failed with status {response.StatusCode}";
                }
                else
                {
                    errorMessage = $"Signup failed with status {response.StatusCode}";
                }
            }
            catch
            {
                errorMessage = $"Signup failed with status {response.StatusCode}";
            }
            
            throw new Exception(errorMessage);
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Network error: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            if (ex.Message.StartsWith("Signup failed") || ex.Message.StartsWith("Network error"))
                throw;
            throw new Exception($"Signup error: {ex.Message}", ex);
        }
    }

    public async Task<AuthTokens?> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            var request = new RefreshTokenRequest { RefreshToken = refreshToken };
            var response = await _httpClient.PostAsJsonAsync("/api/auth/refresh", request);

            if (response.IsSuccessStatusCode)
            {
                var tokens = await response.Content.ReadFromJsonAsync<AuthTokens>();
                if (tokens != null)
                {
                    await _storageHelper.SaveTokensAsync(tokens);
                }
                return tokens;
            }

            var error = await response.Content.ReadFromJsonAsync<ApiError>();
            throw new Exception(error?.Message ?? "Token refresh failed");
        }
        catch (Exception ex)
        {
            throw new Exception($"Token refresh error: {ex.Message}", ex);
        }
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var accessToken = await _storageHelper.GetAccessTokenAsync();
        return !string.IsNullOrEmpty(accessToken);
    }

    public async Task LogoutAsync()
    {
        await _storageHelper.ClearAllAsync();
    }
}
