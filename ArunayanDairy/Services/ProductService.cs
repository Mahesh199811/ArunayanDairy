using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ArunayanDairy.Helpers;
using ArunayanDairy.Models;

namespace ArunayanDairy.Services;

public interface IProductService
{
    Task<List<Category>> GetCategoriesAsync();
    Task<List<Product>> GetProductsAsync(Guid? categoryId = null);
    Task<List<Product>> GetAvailableProductsAsync(DateTime date, Guid? categoryId = null);
    Task<Product?> GetProductByIdAsync(Guid id);
}

public class ProductService : IProductService
{
    private readonly HttpClient _httpClient;
    private readonly SecureStorageHelper _secureStorageHelper;
    private readonly string _baseUrl;

    public ProductService(HttpClient httpClient, SecureStorageHelper secureStorageHelper)
    {
        _httpClient = httpClient;
        _secureStorageHelper = secureStorageHelper;
        _baseUrl = DeviceInfo.Platform == DevicePlatform.Android
            ? "http://10.0.2.2:5001/api"
            : "http://localhost:5001/api";
    }

    private async Task SetAuthHeaderAsync()
    {
        var token = await _secureStorageHelper.GetAccessTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    public async Task<List<Category>> GetCategoriesAsync()
    {
        try
        {
            await SetAuthHeaderAsync();
            var response = await _httpClient.GetAsync($"{_baseUrl}/categories");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Category>>() ?? new List<Category>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting categories: {ex.Message}");
            return new List<Category>();
        }
    }

    public async Task<List<Product>> GetProductsAsync(Guid? categoryId = null)
    {
        try
        {
            await SetAuthHeaderAsync();
            var url = $"{_baseUrl}/products";
            if (categoryId.HasValue)
                url += $"?categoryId={categoryId}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Product>>() ?? new List<Product>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting products: {ex.Message}");
            return new List<Product>();
        }
    }

    public async Task<List<Product>> GetAvailableProductsAsync(DateTime date, Guid? categoryId = null)
    {
        try
        {
            await SetAuthHeaderAsync();
            var dateStr = date.ToString("yyyy-MM-dd");
            var url = $"{_baseUrl}/products/available/{dateStr}";
            if (categoryId.HasValue)
                url += $"?categoryId={categoryId}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Product>>() ?? new List<Product>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting available products: {ex.Message}");
            return new List<Product>();
        }
    }

    public async Task<Product?> GetProductByIdAsync(Guid id)
    {
        try
        {
            await SetAuthHeaderAsync();
            var response = await _httpClient.GetAsync($"{_baseUrl}/products/{id}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Product>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting product: {ex.Message}");
            return null;
        }
    }
}
