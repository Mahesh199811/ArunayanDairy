using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Api.Data;
using OrderService.Api.DTOs;
using OrderService.Api.Models;
using OrderService.Api.Services;

namespace OrderService.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly OrderDbContext _context;
    private readonly ProductServiceClient _productService;

    public OrdersController(
        OrderDbContext context,
        ProductServiceClient productService)
    {
        _context = context;
        _productService = productService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder(
        CreateOrderRequest request)
    {
        if (request.Items == null || request.Items.Count == 0)
        {
            return BadRequest("Order must contain at least one item.");
        }

        if (request.ScheduledDate.Date < DateTime.UtcNow.Date)
        {
            return BadRequest(
                "Scheduled date cannot be in the past.");
        }

        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            ScheduledDate = request.ScheduledDate,
            Status = "Pending"
        };

        decimal totalAmount = 0;

        foreach (var item in request.Items)
        {
            if (item.Quantity <= 0)
            {
                return BadRequest(
                    "Quantity must be greater than zero.");
            }

            var product =
                await _productService.GetProduct(item.ProductId);

            if (product == null)
            {
                return BadRequest(
                    $"Product {item.ProductId} was not found.");
            }

            if (product.AvailableDate.Date !=
                request.ScheduledDate.Date)
            {
                return BadRequest(
                    $"Product {product.Name} is not available on the selected date.");
            }

            if (item.Quantity > product.AvailableQuantity)
            {
                return BadRequest(
                    $"Insufficient quantity for {product.Name}.");
            }

            var itemTotal = product.Price * item.Quantity;

            var orderItem = new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = product.Price,
                Quantity = item.Quantity,
                TotalPrice = itemTotal
            };

            order.Items.Add(orderItem);

            totalAmount += itemTotal;
        }

        order.TotalAmount = totalAmount;

        _context.Orders.Add(order);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetOrder),
            new { id = order.Id },
            order);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetOrder(Guid id)
    {
        var order = await _context.Orders
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (order == null)
        {
            return NotFound();
        }

        return Ok(order);
    }

    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> GetUserOrders(Guid userId)
    {
        var orders = await _context.Orders
            .Include(x => x.Items)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return Ok(orders);
    }
}
