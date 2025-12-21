namespace ArunayanDairy.Models;

/// <summary>
/// Standard error response from API
/// </summary>
public class ApiError
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public object? Details { get; set; }
}
