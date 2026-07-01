using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NaarNoor.Application.Services;
using NaarNoor.Infrastructure.Services;

namespace NaarNoor.API.Controllers;

/// <summary>
/// Authentication endpoints for user login and token generation
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IJwtService _jwtService;
    private readonly IUserService _userService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IJwtService jwtService, IUserService userService, ILogger<AuthController> logger)
    {
        _jwtService = jwtService;
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Login user and generate JWT token
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/auth/login
    ///     {
    ///         "email": "user@example.com",
    ///         "password": "password123"
    ///     }
    /// </remarks>
    /// <param name="request">Login credentials</param>
    /// <returns>JWT token</returns>
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            // ✅ Authenticate against Supabase with actual password verification
            var authResult = await _userService.AuthenticateAsync(request.Email, request.Password);

            if (!authResult.Success)
            {
                _logger.LogWarning("Login failed for {Email}: {Error}", request.Email, authResult.Error);
                return Unauthorized(new { error = authResult.Error ?? "Invalid credentials" });
            }

            var user = authResult.User!;
            var token = _jwtService.GenerateToken(user.Id, user.Email, user.Roles);

            _logger.LogInformation("User {Email} logged in successfully", request.Email);

            return Ok(new
            {
                token,
                type = "Bearer",
                expiresIn = 3600,
                user = new
                {
                    id = user.Id,
                    email = user.Email,
                    fullName = user.FullName,
                    roles = user.Roles
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed for user {Email}", request.Email);
            return StatusCode(500, new { error = "Login failed" });
        }
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/auth/register
    ///     {
    ///         "email": "newuser@example.com",
    ///         "password": "password123",
    ///         "fullName": "John Doe"
    ///     }
    /// </remarks>
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            // ✅ Register user with Supabase
            var registerResult = await _userService.RegisterAsync(request.Email, request.Password, request.FullName);

            if (!registerResult.Success)
            {
                _logger.LogWarning("Registration failed for {Email}: {Error}", request.Email, registerResult.Error);
                return BadRequest(new { error = registerResult.Error ?? "Registration failed" });
            }

            var user = registerResult.User!;
            
            _logger.LogInformation("User {Email} registered successfully", request.Email);

            return Created("", new
            {
                message = "Registration successful. Please log in.",
                user = new
                {
                    id = user.Id,
                    email = user.Email,
                    fullName = user.FullName
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration failed for {Email}", request.Email);
            return StatusCode(500, new { error = "Registration failed" });
        }
    }

    /// <summary>
    /// Refresh JWT token
    /// </summary>
    [Authorize]
    [HttpPost("refresh")]
    public IActionResult RefreshToken()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var roles = User.FindAll(System.Security.Claims.ClaimTypes.Role)?.Select(c => c.Value).ToArray() ?? Array.Empty<string>();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { error = "Invalid token" });

            var newToken = _jwtService.GenerateToken(userId, email ?? "unknown@example.com", roles);

            _logger.LogInformation("Token refreshed for user {UserId}", userId);

            return Ok(new
            {
                token = newToken,
                type = "Bearer",
                expiresIn = 3600
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token refresh failed");
            return StatusCode(500, new { error = "Token refresh failed" });
        }
    }

    /// <summary>
    /// Logout user (client-side: delete token)
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation("User {UserId} logged out", userId);

        return Ok(new { message = "Logged out successfully. Please delete the token on client side." });
    }
}

public class LoginRequest
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}

public class RegisterRequest
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string FullName { get; set; } = "";
}
