using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductService.Api.Data;
using ProductService.Api.DTOs;
using ProductService.Api.Models;

namespace ProductService.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly ProductDbContext _context;

    public ProductsController(ProductDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var products = await _context.Products
            .OrderBy(x => x.AvailableDate)
            .ToListAsync();

        return Ok(products);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProduct(Guid id)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(x => x.Id == id);

        if (product == null)
        {
            return NotFound();
        }

        return Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct(
        CreateProductRequest request)
    {
        if (request.Price <= 0)
        {
            return BadRequest("Price must be greater than zero.");
        }

        if (request.AvailableQuantity < 0)
        {
            return BadRequest(
                "Available quantity cannot be negative.");
        }

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Unit = request.Unit,
            AvailableQuantity = request.AvailableQuantity,
            AvailableDate = request.AvailableDate
        };

        _context.Products.Add(product);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetProduct),
            new { id = product.Id },
            product);
    }

    [HttpPost("{id:guid}/reduce-stock")]
    public async Task<IActionResult> ReduceStock(
        Guid id,
        ReduceStockRequest request)
    {
        if (request.Quantity <= 0)
        {
            return BadRequest("Quantity must be greater than zero.");
        }

        var product = await _context.Products
            .FirstOrDefaultAsync(x => x.Id == id);

        if (product == null)
        {
            return NotFound();
        }

        if (request.Quantity > product.AvailableQuantity)
        {
            return BadRequest(
                $"Insufficient quantity for {product.Name}.");
        }

        product.AvailableQuantity -= request.Quantity;

        await _context.SaveChangesAsync();

        return Ok(product);
    }
}
