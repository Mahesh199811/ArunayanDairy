using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ArunayanDairy.API.Models;
using ArunayanDairy.API.Services;

namespace ArunayanDairy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    #region Product Endpoints

    /// <summary>
    /// Get all products with optional filters
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts([FromQuery] Guid? categoryId = null, [FromQuery] bool? isActive = null)
    {
        try
        {
            var products = await _productService.GetProductsAsync(categoryId, isActive);
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products");
            return StatusCode(500, new ApiError
            {
                Code = "INTERNAL_ERROR",
                Message = "An error occurred while retrieving products"
            });
        }
    }

    /// <summary>
    /// Get available products for a specific date
    /// </summary>
    [HttpGet("available/{date}")]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailableProducts(DateTime date, [FromQuery] Guid? categoryId = null, [FromQuery] Guid? vendorId = null)
    {
        try
        {
            var products = await _productService.GetAvailableProductsAsync(date, categoryId, vendorId);
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available products for date {Date}", date);
            return StatusCode(500, new ApiError
            {
                Code = "INTERNAL_ERROR",
                Message = "An error occurred while retrieving available products"
            });
        }
    }

    /// <summary>
    /// Get product details by ID with availability
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductById(Guid id)
    {
        try
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound(new ApiError
                {
                    Code = "PRODUCT_NOT_FOUND",
                    Message = "Product not found"
                });
            }

            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product {ProductId}", id);
            return StatusCode(500, new ApiError
            {
                Code = "INTERNAL_ERROR",
                Message = "An error occurred while retrieving the product"
            });
        }
    }

    /// <summary>
    /// Create a new product (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
    {
        if (!ModelState.IsValid)
        {
            return UnprocessableEntity(new ApiError
            {
                Code = "VALIDATION_ERROR",
                Message = "Invalid request data",
                Details = ModelState
            });
        }

        try
        {
            var product = await _productService.CreateProductAsync(dto);
            return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiError
            {
                Code = "OPERATION_FAILED",
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return StatusCode(500, new ApiError
            {
                Code = "INTERNAL_ERROR",
                Message = "An error occurred while creating the product"
            });
        }
    }

    /// <summary>
    /// Update an existing product (Admin only)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] CreateProductDto dto)
    {
        if (!ModelState.IsValid)
        {
            return UnprocessableEntity(new ApiError
            {
                Code = "VALIDATION_ERROR",
                Message = "Invalid request data",
                Details = ModelState
            });
        }

        try
        {
            var product = await _productService.UpdateProductAsync(id, dto);
            if (product == null)
            {
                return NotFound(new ApiError
                {
                    Code = "PRODUCT_NOT_FOUND",
                    Message = "Product not found"
                });
            }

            return Ok(product);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiError
            {
                Code = "OPERATION_FAILED",
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId}", id);
            return StatusCode(500, new ApiError
            {
                Code = "INTERNAL_ERROR",
                Message = "An error occurred while updating the product"
            });
        }
    }

    /// <summary>
    /// Delete a product (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        try
        {
            var result = await _productService.DeleteProductAsync(id);
            if (!result)
            {
                return NotFound(new ApiError
                {
                    Code = "PRODUCT_NOT_FOUND",
                    Message = "Product not found or cannot be deleted"
                });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {ProductId}", id);
            return StatusCode(500, new ApiError
            {
                Code = "INTERNAL_ERROR",
                Message = "An error occurred while deleting the product"
            });
        }
    }

    #endregion

    #region Product Availability Endpoints

    /// <summary>
    /// Get product availability for date range
    /// </summary>
    [HttpGet("{id}/availability")]
    [ProducesResponseType(typeof(IEnumerable<ProductAvailabilityDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProductAvailability(Guid id, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var availabilities = await _productService.GetProductAvailabilityAsync(id, fromDate, toDate);
            return Ok(availabilities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving availability for product {ProductId}", id);
            return StatusCode(500, new ApiError
            {
                Code = "INTERNAL_ERROR",
                Message = "An error occurred while retrieving product availability"
            });
        }
    }

    /// <summary>
    /// Create or update product availability (Admin only)
    /// </summary>
    [HttpPost("availability")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(typeof(ProductAvailabilityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetProductAvailability([FromBody] CreateProductAvailabilityDto dto)
    {
        if (!ModelState.IsValid)
        {
            return UnprocessableEntity(new ApiError
            {
                Code = "VALIDATION_ERROR",
                Message = "Invalid request data",
                Details = ModelState
            });
        }

        try
        {
            var availability = await _productService.CreateOrUpdateAvailabilityAsync(dto);
            return Ok(availability);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiError
            {
                Code = "OPERATION_FAILED",
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting product availability");
            return StatusCode(500, new ApiError
            {
                Code = "INTERNAL_ERROR",
                Message = "An error occurred while setting product availability"
            });
        }
    }

    /// <summary>
    /// Delete product availability (Admin only)
    /// </summary>
    [HttpDelete("availability/{id}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAvailability(Guid id)
    {
        try
        {
            var result = await _productService.DeleteAvailabilityAsync(id);
            if (!result)
            {
                return NotFound(new ApiError
                {
                    Code = "AVAILABILITY_NOT_FOUND",
                    Message = "Availability record not found"
                });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting availability {AvailabilityId}", id);
            return StatusCode(500, new ApiError
            {
                Code = "INTERNAL_ERROR",
                Message = "An error occurred while deleting availability"
            });
        }
    }

    #endregion
}
