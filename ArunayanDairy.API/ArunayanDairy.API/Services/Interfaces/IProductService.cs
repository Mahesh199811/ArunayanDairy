using ArunayanDairy.API.DTOs.Products;

namespace ArunayanDairy.API.Services.Interfaces;

public interface IProductService
{
    Task<ProductResponseDto?> GetByIdAsync(string id);
    Task<IEnumerable<ProductResponseDto>> GetAllAsync();
    Task<IEnumerable<ProductResponseDto>> GetByCategoryAsync(string category);
    Task<ProductResponseDto> CreateAsync(ProductCreateDto productDto);
    Task<ProductResponseDto> UpdateAsync(string id, ProductCreateDto productDto);
    Task DeleteAsync(string id);
}
