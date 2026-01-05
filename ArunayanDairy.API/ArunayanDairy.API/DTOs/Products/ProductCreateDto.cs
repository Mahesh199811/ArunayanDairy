using System.ComponentModel.DataAnnotations;

namespace ArunayanDairy.API.DTOs.Products;

public class ProductCreateDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    [Required]
    public string Unit { get; set; } = string.Empty;

    [Required]
    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    [Required]
    public string Category { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;

    public bool IsAvailable { get; set; } = true;
}
