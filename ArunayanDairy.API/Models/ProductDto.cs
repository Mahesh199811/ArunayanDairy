using System.ComponentModel.DataAnnotations;

namespace ArunayanDairy.API.Models;

/// <summary>
/// DTO for product list responses
/// </summary>
public class ProductDto
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public decimal MinOrderQuantity { get; set; }
    public decimal? MaxOrderQuantity { get; set; }
}

/// <summary>
/// DTO for product detail with availability
/// </summary>
public class ProductDetailDto : ProductDto
{
    public List<ProductAvailabilityDto> Availabilities { get; set; } = new();
}

/// <summary>
/// DTO for product availability
/// </summary>
public class ProductAvailabilityDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public DateTime AvailableDate { get; set; }
    public decimal StockQuantity { get; set; }
    public decimal EffectivePrice { get; set; }
    public bool IsAvailable { get; set; }
}

/// <summary>
/// DTO for creating/updating products (Admin only)
/// </summary>
public class CreateProductDto
{
    [Required]
    public Guid CategoryId { get; set; }

    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string SKU { get; set; } = string.Empty;

    [Required]
    public string Unit { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal BasePrice { get; set; }

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal MinOrderQuantity { get; set; } = 1;

    [Range(0.01, double.MaxValue)]
    public decimal? MaxOrderQuantity { get; set; }
}

/// <summary>
/// DTO for creating/updating product availability (Admin only)
/// </summary>
public class CreateProductAvailabilityDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    public DateTime AvailableDate { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal StockQuantity { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal? PriceOverride { get; set; }

    public bool IsAvailable { get; set; } = true;
}
