using System.Net.Http.Json;

namespace OrderService.Api.Services;

public class ProductServiceClient
{
    private readonly HttpClient _httpClient;

    public ProductServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ProductResponse?> GetProduct(Guid productId)
    {
        var response = await _httpClient.GetAsync($"api/products/{productId}");

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<ProductResponse>();
    }
}

public class ProductResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string Unit { get; set; } = string.Empty;

    public decimal AvailableQuantity { get; set; }

    public DateTime AvailableDate { get; set; }
}
