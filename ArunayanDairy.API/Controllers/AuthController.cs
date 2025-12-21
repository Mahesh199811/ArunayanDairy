using Microsoft.AspNetCore.Mvc;
using ArunayanDairy.API.Models;
using ArunayanDairy.API.Services;

namespace ArunayanDairy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// User login endpoint
    /// </summary>
    /// <param name="loginDto">Email and password</param>
    /// <returns>User info and JWT tokens</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
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
            var response = await _authService.LoginAsync(loginDto);

            if (response == null)
            {
                return Unauthorized(new ApiError
                {
                    Code = "INVALID_CREDENTIALS",
                    Message = "Invalid email or password"
                });
            }

            _logger.LogInformation("User {Email} logged in successfully", loginDto.Email);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for {Email}", loginDto.Email);
            return StatusCode(500, new ApiError
            {
                Code = "INTERNAL_ERROR",
                Message = "An error occurred during login"
            });
        }
    }

    /// <summary>
    /// User signup endpoint
    /// </summary>
    /// <param name="signupDto">Name, email, and password</param>
    /// <returns>User info and JWT tokens</returns>
    [HttpPost("signup")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Signup([FromBody] SignupDto signupDto)
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
            var response = await _authService.SignupAsync(signupDto);

            if (response == null)
            {
                return Conflict(new ApiError
                {
                    Code = "EMAIL_ALREADY_EXISTS",
                    Message = "An account with this email already exists"
                });
            }

            _logger.LogInformation("New user {Email} signed up successfully", signupDto.Email);
            return CreatedAtAction(nameof(Signup), response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during signup for {Email}", signupDto.Email);
            return StatusCode(500, new ApiError
            {
                Code = "INTERNAL_ERROR",
                Message = "An error occurred during signup"
            });
        }
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    /// <param name="refreshTokenDto">Refresh token</param>
    /// <returns>New access and refresh tokens</returns>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(TokensDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto refreshTokenDto)
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
            var tokens = await _authService.RefreshTokenAsync(refreshTokenDto.RefreshToken);

            if (tokens == null)
            {
                return Unauthorized(new ApiError
                {
                    Code = "INVALID_REFRESH_TOKEN",
                    Message = "Invalid or expired refresh token"
                });
            }

            _logger.LogInformation("Tokens refreshed successfully");
            return Ok(tokens);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return StatusCode(500, new ApiError
            {
                Code = "INTERNAL_ERROR",
                Message = "An error occurred during token refresh"
            });
        }
    }
}
