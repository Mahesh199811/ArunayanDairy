namespace ArunayanDairy.API.Models;

/// <summary>
/// Standard error response matching the API contract
/// </summary>
public class ApiError
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public object? Details { get; set; }
}
