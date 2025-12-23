using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ArunayanDairy.API.Data;
using ArunayanDairy.API.Models;

namespace ArunayanDairy.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class VendorsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<VendorsController> _logger;

    public VendorsController(ApplicationDbContext context, ILogger<VendorsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all vendors/admins
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<VendorDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVendors()
    {
        try
        {
            var vendors = await _context.Users
                .Where(u => u.Role == "admin")
                .Select(u => new VendorDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email
                })
                .ToListAsync();

            return Ok(vendors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vendors");
            return StatusCode(500, new ApiError
            {
                Code = "INTERNAL_ERROR",
                Message = "An error occurred while retrieving vendors"
            });
        }
    }
}

public class VendorDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
