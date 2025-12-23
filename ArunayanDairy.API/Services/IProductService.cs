using ArunayanDairy.API.Models;

namespace ArunayanDairy.API.Services;

/// <summary>
/// Service interface for product and category management
/// </summary>
public interface IProductService
{
    // Category operations
    Task<IEnumerable<CategoryDto>> GetCategoriesAsync();
    Task<CategoryDto?> GetCategoryByIdAsync(Guid id);
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto);
    Task<CategoryDto?> UpdateCategoryAsync(Guid id, CreateCategoryDto dto);
    Task<bool> DeleteCategoryAsync(Guid id);

    // Product operations
    Task<IEnumerable<ProductDto>> GetProductsAsync(Guid? categoryId = null, bool? isActive = null);
    Task<ProductDetailDto?> GetProductByIdAsync(Guid id);
    Task<IEnumerable<ProductDto>> GetAvailableProductsAsync(DateTime date, Guid? categoryId = null);
    Task<ProductDto> CreateProductAsync(CreateProductDto dto);
    Task<ProductDto?> UpdateProductAsync(Guid id, CreateProductDto dto);
    Task<bool> DeleteProductAsync(Guid id);

    // Product availability operations
    Task<IEnumerable<ProductAvailabilityDto>> GetProductAvailabilityAsync(Guid productId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<ProductAvailabilityDto> CreateOrUpdateAvailabilityAsync(CreateProductAvailabilityDto dto);
    Task<bool> DeleteAvailabilityAsync(Guid id);
}
