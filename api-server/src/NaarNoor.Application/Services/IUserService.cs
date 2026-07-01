namespace NaarNoor.Application.Services;

/// <summary>
/// User service for authentication and user management
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Authenticate user with email and password
    /// </summary>
    Task<UserAuthResult> AuthenticateAsync(string email, string password);

    /// <summary>
    /// Register new user
    /// </summary>
    Task<UserAuthResult> RegisterAsync(string email, string password, string fullName);

    /// <summary>
    /// Get user by ID
    /// </summary>
    Task<UserDto?> GetUserByIdAsync(string userId);

    /// <summary>
    /// Get user by email
    /// </summary>
    Task<UserDto?> GetUserByEmailAsync(string email);

    /// <summary>
    /// Change user password
    /// </summary>
    Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
}

public class UserAuthResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public UserDto? User { get; set; }
}

public class UserDto
{
    public string Id { get; set; } = "";
    public string Email { get; set; } = "";
    public string FullName { get; set; } = "";
    public string[] Roles { get; set; } = Array.Empty<string>();
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}
