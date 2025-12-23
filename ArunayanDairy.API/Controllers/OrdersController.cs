using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ArunayanDairy.API.Models;
using ArunayanDairy.API.Services;

namespace ArunayanDairy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    /// <summary>
    /// Place a new order (Customer)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
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
            var customerId = GetCurrentUserId();
            var order = await _orderService.CreateOrderAsync(customerId, dto);
            return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiError
            {
                Code = "ORDER_FAILED",
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return StatusCode(500, new ApiError
            {
                Code = "INTERNAL_ERROR",
                Message = "An error occurred while creating the order"
            });
        }
    }

    /// <summary>
    /// Get customer's orders
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyOrders([FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var customerId = GetCurrentUserId();
            var orders = await _orderService.GetCustomerOrdersAsync(customerId, fromDate, toDate);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer orders");
            return StatusCode(500, new ApiError
            {
                Code = "INTERNAL_ERROR",
                Message = "An error occurred while retrieving orders"
            });
        }
    }

    /// <summary>
    /// Get all orders (Admin only)
    /// </summary>
    [HttpGet("all")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllOrders(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? status = null)
    {
        try
        {
            OrderStatus? orderStatus = null;
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, true, out var parsedStatus))
            {
                orderStatus = parsedStatus;
            }

            var orders = await _orderService.GetAllOrdersAsync(fromDate, toDate, orderStatus);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all orders");
            return StatusCode(500, new ApiError
            {
                Code = "INTERNAL_ERROR",
                Message = "An error occurred while retrieving orders"
            });
        }
    }

    /// <summary>
    /// Get order details by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetOrderById(Guid id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var isAdmin = User.IsInRole("admin");

            var order = await _orderService.GetOrderByIdAsync(id, isAdmin ? null : currentUserId);
            
            if (order == null)
            {
                return NotFound(new ApiError
                {
                    Code = "ORDER_NOT_FOUND",
                    Message = "Order not found or you don't have permission to view it"
                });
            }

            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order {OrderId}", id);
            return StatusCode(500, new ApiError
            {
                Code = "INTERNAL_ERROR",
                Message = "An error occurred while retrieving the order"
            });
        }
    }

    /// <summary>
    /// Update order status (Admin only)
    /// </summary>
    [HttpPut("{id}/status")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusDto dto)
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
            if (!Enum.TryParse<OrderStatus>(dto.Status, true, out var newStatus))
            {
                return BadRequest(new ApiError
                {
                    Code = "INVALID_STATUS",
                    Message = "Invalid order status"
                });
            }

            var order = await _orderService.UpdateOrderStatusAsync(id, newStatus);
            if (order == null)
            {
                return NotFound(new ApiError
                {
                    Code = "ORDER_NOT_FOUND",
                    Message = "Order not found"
                });
            }

            return Ok(order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiError
            {
                Code = "STATUS_UPDATE_FAILED",
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order status for {OrderId}", id);
            return StatusCode(500, new ApiError
            {
                Code = "INTERNAL_ERROR",
                Message = "An error occurred while updating the order status"
            });
        }
    }

    /// <summary>
    /// Cancel order (Customer - only for Pending/Confirmed status)
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelOrder(Guid id)
    {
        try
        {
            var customerId = GetCurrentUserId();
            var result = await _orderService.CancelOrderAsync(id, customerId);
            
            if (!result)
            {
                return NotFound(new ApiError
                {
                    Code = "ORDER_NOT_FOUND",
                    Message = "Order not found or you don't have permission to cancel it"
                });
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiError
            {
                Code = "CANCELLATION_FAILED",
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling order {OrderId}", id);
            return StatusCode(500, new ApiError
            {
                Code = "INTERNAL_ERROR",
                Message = "An error occurred while cancelling the order"
            });
        }
    }

    /// <summary>
    /// Get order summary statistics (Admin only)
    /// </summary>
    [HttpGet("summary")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(typeof(OrderSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrderSummary([FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var summary = await _orderService.GetOrderSummaryAsync(fromDate, toDate);
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order summary");
            return StatusCode(500, new ApiError
            {
                Code = "INTERNAL_ERROR",
                Message = "An error occurred while retrieving order summary"
            });
        }
    }

    #region Helper Methods

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user token");
        }

        return userId;
    }

    #endregion
}
