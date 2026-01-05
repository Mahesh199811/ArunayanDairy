using ArunayanDairy.API.DTOs.Products;
using ArunayanDairy.API.Models;
using ArunayanDairy.API.Repositories.Interfaces;
using ArunayanDairy.API.Services.Interfaces;

namespace ArunayanDairy.API.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<ProductResponseDto?> GetByIdAsync(string id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null) return null;

        return MapToResponseDto(product);
    }

    public async Task<IEnumerable<ProductResponseDto>> GetAllAsync()
    {
        var products = await _productRepository.GetAllAsync();
        return products.Select(MapToResponseDto);
    }

    public async Task<IEnumerable<ProductResponseDto>> GetByCategoryAsync(string category)
    {
        var products = await _productRepository.GetByCategoryAsync(category);
        return products.Select(MapToResponseDto);
    }

    public async Task<ProductResponseDto> CreateAsync(ProductCreateDto productDto)
    {
        var product = new Product
        {
            Name = productDto.Name,
            Description = productDto.Description,
            Price = productDto.Price,
            Unit = productDto.Unit,
            StockQuantity = productDto.StockQuantity,
            Category = productDto.Category,
            ImageUrl = productDto.ImageUrl,
            IsAvailable = productDto.IsAvailable
        };

        await _productRepository.CreateAsync(product);
        return MapToResponseDto(product);
    }

    public async Task<ProductResponseDto> UpdateAsync(string id, ProductCreateDto productDto)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
        {
            throw new KeyNotFoundException($"Product with id {id} not found");
        }

        product.Name = productDto.Name;
        product.Description = productDto.Description;
        product.Price = productDto.Price;
        product.Unit = productDto.Unit;
        product.StockQuantity = productDto.StockQuantity;
        product.Category = productDto.Category;
        product.ImageUrl = productDto.ImageUrl;
        product.IsAvailable = productDto.IsAvailable;

        await _productRepository.UpdateAsync(product);
        return MapToResponseDto(product);
    }

    public async Task DeleteAsync(string id)
    {
        await _productRepository.DeleteAsync(id);
    }

    private static ProductResponseDto MapToResponseDto(Product product)
    {
        return new ProductResponseDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Unit = product.Unit,
            StockQuantity = product.StockQuantity,
            Category = product.Category,
            ImageUrl = product.ImageUrl,
            IsAvailable = product.IsAvailable,
            CreatedAt = product.CreatedAt
        };
    }
}
