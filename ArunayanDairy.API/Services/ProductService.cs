using Microsoft.EntityFrameworkCore;
using ArunayanDairy.API.Models;
using ArunayanDairy.API.Repositories;

namespace ArunayanDairy.API.Services;

/// <summary>
/// Service implementation for product and category management
/// </summary>
public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IUnitOfWork unitOfWork, ILogger<ProductService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    #region Category Operations

    public async Task<IEnumerable<CategoryDto>> GetCategoriesAsync()
    {
        var categories = await _unitOfWork.Categories.GetAllAsync();
        return categories
            .OrderBy(c => c.DisplayOrder)
            .Select(MapToCategoryDto);
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(Guid id)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        return category == null ? null : MapToCategoryDto(category);
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto)
    {
        var category = new Category
        {
            Name = dto.Name,
            Description = dto.Description,
            DisplayOrder = dto.DisplayOrder,
            IsActive = dto.IsActive
        };

        await _unitOfWork.Categories.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Created category {CategoryName} with ID {CategoryId}", category.Name, category.Id);

        return MapToCategoryDto(category);
    }

    public async Task<CategoryDto?> UpdateCategoryAsync(Guid id, CreateCategoryDto dto)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category == null)
            return null;

        category.Name = dto.Name;
        category.Description = dto.Description;
        category.DisplayOrder = dto.DisplayOrder;
        category.IsActive = dto.IsActive;
        category.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Categories.Update(category);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Updated category {CategoryId}", id);

        return MapToCategoryDto(category);
    }

    public async Task<bool> DeleteCategoryAsync(Guid id)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category == null)
            return false;

        // Check if category has products
        var hasProducts = await _unitOfWork.Products.AnyAsync(p => p.CategoryId == id);
        if (hasProducts)
        {
            _logger.LogWarning("Cannot delete category {CategoryId} - has associated products", id);
            return false;
        }

        _unitOfWork.Categories.Remove(category);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Deleted category {CategoryId}", id);

        return true;
    }

    #endregion

    #region Product Operations

    public async Task<IEnumerable<ProductDto>> GetProductsAsync(Guid? categoryId = null, bool? isActive = null)
    {
        var products = await _unitOfWork.Products.FindAsync(p =>
            (!categoryId.HasValue || p.CategoryId == categoryId.Value) &&
            (!isActive.HasValue || p.IsActive == isActive.Value));

        var productList = products.ToList();
        
        // Load categories for mapping
        var categoryIds = productList.Select(p => p.CategoryId).Distinct();
        var categories = await _unitOfWork.Categories.FindAsync(c => categoryIds.Contains(c.Id));
        var categoryDict = categories.ToDictionary(c => c.Id, c => c.Name);

        return productList.Select(p => MapToProductDto(p, categoryDict.GetValueOrDefault(p.CategoryId, "Unknown")));
    }

    public async Task<ProductDetailDto?> GetProductByIdAsync(Guid id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null)
            return null;

        var category = await _unitOfWork.Categories.GetByIdAsync(product.CategoryId);
        var availabilities = await _unitOfWork.ProductAvailabilities.FindAsync(pa => pa.ProductId == id);

        var productDto = MapToProductDetailDto(product, category?.Name ?? "Unknown");
        productDto.Availabilities = availabilities
            .OrderBy(a => a.AvailableDate)
            .Select(a => new ProductAvailabilityDto
            {
                Id = a.Id,
                ProductId = a.ProductId,
                AvailableDate = a.AvailableDate,
                StockQuantity = a.StockQuantity,
                EffectivePrice = a.PriceOverride ?? product.BasePrice,
                IsAvailable = a.IsAvailable && a.StockQuantity > 0
            })
            .ToList();

        return productDto;
    }

    public async Task<IEnumerable<ProductDto>> GetAvailableProductsAsync(DateTime date, Guid? categoryId = null, Guid? vendorId = null)
    {
        var dateOnly = date.Date;

        var products = await _unitOfWork.Products.FindAsync(p =>
            p.IsActive &&
            (!categoryId.HasValue || p.CategoryId == categoryId.Value) &&
            (!vendorId.HasValue || p.VendorId == vendorId.Value));

        var productList = products.ToList();
        var productIds = productList.Select(p => p.Id).ToList();

        var availabilities = await _unitOfWork.ProductAvailabilities.FindAsync(pa =>
            productIds.Contains(pa.ProductId) &&
            pa.AvailableDate.Date == dateOnly &&
            pa.IsAvailable &&
            pa.StockQuantity > 0);

        var availableProductIds = availabilities.Select(a => a.ProductId).ToHashSet();
        var availableProducts = productList.Where(p => availableProductIds.Contains(p.Id));

        var categoryIds = availableProducts.Select(p => p.CategoryId).Distinct();
        var categories = await _unitOfWork.Categories.FindAsync(c => categoryIds.Contains(c.Id));
        var categoryDict = categories.ToDictionary(c => c.Id, c => c.Name);

        return availableProducts.Select(p => MapToProductDto(p, categoryDict.GetValueOrDefault(p.CategoryId, "Unknown")));
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
    {
        // Check if SKU already exists
        var existingProduct = await _unitOfWork.Products.FirstOrDefaultAsync(p => p.SKU == dto.SKU);
        if (existingProduct != null)
            throw new InvalidOperationException($"Product with SKU '{dto.SKU}' already exists");

        // Validate category exists
        var category = await _unitOfWork.Categories.GetByIdAsync(dto.CategoryId);
        if (category == null)
            throw new InvalidOperationException($"Category with ID '{dto.CategoryId}' not found");

        var product = new Product
        {
            CategoryId = dto.CategoryId,
            Name = dto.Name,
            Description = dto.Description,
            SKU = dto.SKU,
            Unit = Enum.Parse<ProductUnit>(dto.Unit),
            BasePrice = dto.BasePrice,
            ImageUrl = dto.ImageUrl,
            IsActive = dto.IsActive,
            MinOrderQuantity = dto.MinOrderQuantity,
            MaxOrderQuantity = dto.MaxOrderQuantity
        };

        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Created product {ProductName} with SKU {SKU}", product.Name, product.SKU);

        return MapToProductDto(product, category.Name);
    }

    public async Task<ProductDto?> UpdateProductAsync(Guid id, CreateProductDto dto)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null)
            return null;

        // Check if SKU is being changed and already exists
        if (product.SKU != dto.SKU)
        {
            var existingProduct = await _unitOfWork.Products.FirstOrDefaultAsync(p => p.SKU == dto.SKU);
            if (existingProduct != null)
                throw new InvalidOperationException($"Product with SKU '{dto.SKU}' already exists");
        }

        // Validate category exists
        var category = await _unitOfWork.Categories.GetByIdAsync(dto.CategoryId);
        if (category == null)
            throw new InvalidOperationException($"Category with ID '{dto.CategoryId}' not found");

        product.CategoryId = dto.CategoryId;
        product.Name = dto.Name;
        product.Description = dto.Description;
        product.SKU = dto.SKU;
        product.Unit = Enum.Parse<ProductUnit>(dto.Unit);
        product.BasePrice = dto.BasePrice;
        product.ImageUrl = dto.ImageUrl;
        product.IsActive = dto.IsActive;
        product.MinOrderQuantity = dto.MinOrderQuantity;
        product.MaxOrderQuantity = dto.MaxOrderQuantity;
        product.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Updated product {ProductId}", id);

        return MapToProductDto(product, category.Name);
    }

    public async Task<bool> DeleteProductAsync(Guid id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null)
            return false;

        // Check if product has orders
        var hasOrders = await _unitOfWork.OrderItems.AnyAsync(oi => oi.ProductId == id);
        if (hasOrders)
        {
            _logger.LogWarning("Cannot delete product {ProductId} - has associated orders", id);
            return false;
        }

        // Delete associated availabilities
        var availabilities = await _unitOfWork.ProductAvailabilities.FindAsync(pa => pa.ProductId == id);
        _unitOfWork.ProductAvailabilities.RemoveRange(availabilities);

        _unitOfWork.Products.Remove(product);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Deleted product {ProductId}", id);

        return true;
    }

    #endregion

    #region Product Availability Operations

    public async Task<IEnumerable<ProductAvailabilityDto>> GetProductAvailabilityAsync(Guid productId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(productId);
        if (product == null)
            return Enumerable.Empty<ProductAvailabilityDto>();

        var availabilities = await _unitOfWork.ProductAvailabilities.FindAsync(pa =>
            pa.ProductId == productId &&
            (!fromDate.HasValue || pa.AvailableDate.Date >= fromDate.Value.Date) &&
            (!toDate.HasValue || pa.AvailableDate.Date <= toDate.Value.Date));

        return availabilities
            .OrderBy(a => a.AvailableDate)
            .Select(a => new ProductAvailabilityDto
            {
                Id = a.Id,
                ProductId = a.ProductId,
                AvailableDate = a.AvailableDate,
                StockQuantity = a.StockQuantity,
                EffectivePrice = a.PriceOverride ?? product.BasePrice,
                IsAvailable = a.IsAvailable && a.StockQuantity > 0
            });
    }

    public async Task<ProductAvailabilityDto> CreateOrUpdateAvailabilityAsync(CreateProductAvailabilityDto dto)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId);
        if (product == null)
            throw new InvalidOperationException($"Product with ID '{dto.ProductId}' not found");

        var dateOnly = dto.AvailableDate.Date;

        // Check if availability already exists for this date
        var existing = await _unitOfWork.ProductAvailabilities.FirstOrDefaultAsync(pa =>
            pa.ProductId == dto.ProductId && pa.AvailableDate.Date == dateOnly);

        if (existing != null)
        {
            // Update existing
            existing.StockQuantity = dto.StockQuantity;
            existing.PriceOverride = dto.PriceOverride;
            existing.IsAvailable = dto.IsAvailable;
            existing.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.ProductAvailabilities.Update(existing);
        }
        else
        {
            // Create new
            existing = new ProductAvailability
            {
                ProductId = dto.ProductId,
                AvailableDate = dateOnly,
                StockQuantity = dto.StockQuantity,
                PriceOverride = dto.PriceOverride,
                IsAvailable = dto.IsAvailable
            };

            await _unitOfWork.ProductAvailabilities.AddAsync(existing);
        }

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Created/updated availability for product {ProductId} on {Date}", dto.ProductId, dateOnly);

        return new ProductAvailabilityDto
        {
            Id = existing.Id,
            ProductId = existing.ProductId,
            AvailableDate = existing.AvailableDate,
            StockQuantity = existing.StockQuantity,
            EffectivePrice = existing.PriceOverride ?? product.BasePrice,
            IsAvailable = existing.IsAvailable && existing.StockQuantity > 0
        };
    }

    public async Task<bool> DeleteAvailabilityAsync(Guid id)
    {
        var availability = await _unitOfWork.ProductAvailabilities.GetByIdAsync(id);
        if (availability == null)
            return false;

        _unitOfWork.ProductAvailabilities.Remove(availability);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Deleted availability {AvailabilityId}", id);

        return true;
    }

    #endregion

    #region Mapping Helpers

    private static CategoryDto MapToCategoryDto(Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive
        };
    }

    private static ProductDto MapToProductDto(Product product, string categoryName)
    {
        return new ProductDto
        {
            Id = product.Id,
            CategoryId = product.CategoryId,
            CategoryName = categoryName,
            Name = product.Name,
            Description = product.Description,
            SKU = product.SKU,
            Unit = product.Unit.ToString(),
            BasePrice = product.BasePrice,
            ImageUrl = product.ImageUrl,
            IsActive = product.IsActive,
            MinOrderQuantity = product.MinOrderQuantity,
            MaxOrderQuantity = product.MaxOrderQuantity
        };
    }

    private static ProductDetailDto MapToProductDetailDto(Product product, string categoryName)
    {
        return new ProductDetailDto
        {
            Id = product.Id,
            CategoryId = product.CategoryId,
            CategoryName = categoryName,
            Name = product.Name,
            Description = product.Description,
            SKU = product.SKU,
            Unit = product.Unit.ToString(),
            BasePrice = product.BasePrice,
            ImageUrl = product.ImageUrl,
            IsActive = product.IsActive,
            MinOrderQuantity = product.MinOrderQuantity,
            MaxOrderQuantity = product.MaxOrderQuantity
        };
    }

    #endregion
}
