using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using ArunayanDairy.Helpers;
using ArunayanDairy.Models;

namespace ArunayanDairy.Services;

public interface IOrderService
{
    Task<Order?> CreateOrderAsync(CreateOrderRequest request);
    Task<List<Order>> GetMyOrdersAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<Order?> GetOrderByIdAsync(Guid orderId);
    Task<bool> CancelOrderAsync(Guid orderId);
}

public class OrderService : IOrderService
{
    private readonly HttpClient _httpClient;
    private readonly SecureStorageHelper _secureStorageHelper;
    private readonly string _baseUrl;

    public OrderService(HttpClient httpClient, SecureStorageHelper secureStorageHelper)
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

    public async Task<Order?> CreateOrderAsync(CreateOrderRequest request)
    {
        try
        {
            await SetAuthHeaderAsync();
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync($"{_baseUrl}/orders", content);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Order>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error creating order: {ex.Message}");
            return null;
        }
    }

    public async Task<List<Order>> GetMyOrdersAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            await SetAuthHeaderAsync();
            var url = $"{_baseUrl}/orders";
            var queryParams = new List<string>();
            
            if (fromDate.HasValue)
                queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
            if (toDate.HasValue)
                queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");
            
            if (queryParams.Any())
                url += "?" + string.Join("&", queryParams);

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Order>>() ?? new List<Order>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting orders: {ex.Message}");
            return new List<Order>();
        }
    }

    public async Task<Order?> GetOrderByIdAsync(Guid orderId)
    {
        try
        {
            await SetAuthHeaderAsync();
            var response = await _httpClient.GetAsync($"{_baseUrl}/orders/{orderId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Order>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting order: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> CancelOrderAsync(Guid orderId)
    {
        try
        {
            await SetAuthHeaderAsync();
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/orders/{orderId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error cancelling order: {ex.Message}");
            return false;
        }
    }
}
