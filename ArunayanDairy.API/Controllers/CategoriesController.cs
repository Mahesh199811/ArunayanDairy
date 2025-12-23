using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ArunayanDairy.API.Models;
using ArunayanDairy.API.Services;

namespace ArunayanDairy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(IProductService productService, ILogger<CategoriesController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Get all categories
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories()
    {
        try
        {
            var categories = await _productService.GetCategoriesAsync();
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving categories");
            return StatusCode(500, new ApiError
            {
                Code = "INTERNAL_ERROR",
                Message = "An error occurred while retrieving categories"
            });
        }
    }

    /// <summary>
    /// Get category by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategoryById(Guid id)
    {
        try
        {
            var category = await _productService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound(new ApiError
                {
                    Code = "CATEGORY_NOT_FOUND",
                    Message = "Category not found"
                });
            }

            return Ok(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving category {CategoryId}", id);
            return StatusCode(500, new ApiError
            {
                Code = "INTERNAL_ERROR",
                Message = "An error occurred while retrieving the category"
            });
        }
    }

    /// <summary>
    /// Create a new category (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
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
            var category = await _productService.CreateCategoryAsync(dto);
            return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            return StatusCode(500, new ApiError
            {
                Code = "INTERNAL_ERROR",
                Message = "An error occurred while creating the category"
            });
        }
    }

    /// <summary>
    /// Update an existing category (Admin only)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] CreateCategoryDto dto)
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
            var category = await _productService.UpdateCategoryAsync(id, dto);
            if (category == null)
            {
                return NotFound(new ApiError
                {
                    Code = "CATEGORY_NOT_FOUND",
                    Message = "Category not found"
                });
            }

            return Ok(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category {CategoryId}", id);
            return StatusCode(500, new ApiError
            {
                Code = "INTERNAL_ERROR",
                Message = "An error occurred while updating the category"
            });
        }
    }

    /// <summary>
    /// Delete a category (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        try
        {
            var result = await _productService.DeleteCategoryAsync(id);
            if (!result)
            {
                return BadRequest(new ApiError
                {
                    Code = "CATEGORY_HAS_PRODUCTS",
                    Message = "Cannot delete category with associated products"
                });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category {CategoryId}", id);
            return StatusCode(500, new ApiError
            {
                Code = "INTERNAL_ERROR",
                Message = "An error occurred while deleting the category"
            });
        }
    }
}
