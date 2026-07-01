namespace NaarNoor.Infrastructure.Services;

/// <summary>
/// Supabase authentication service interface
/// </summary>
public interface ISupabaseService
{
    /// <summary>
    /// Sign in user with email and password
    /// </summary>
    Task<SupabaseAuthResult> SignInAsync(string email, string password);

    /// <summary>
    /// Sign up new user
    /// </summary>
    Task<SupabaseAuthResult> SignUpAsync(string email, string password, string fullName);

    /// <summary>
    /// Get user by ID
    /// </summary>
    Task<SupabaseUser?> GetUserAsync(string userId);

    /// <summary>
    /// Get user by email
    /// </summary>
    Task<SupabaseUser?> GetUserByEmailAsync(string email);

    /// <summary>
    /// Update user password
    /// </summary>
    Task<SupabaseResult> UpdatePasswordAsync(string userId, string currentPassword, string newPassword);

    /// <summary>
    /// Verify user email
    /// </summary>
    Task<SupabaseResult> VerifyEmailAsync(string userId);
}

public class SupabaseAuthResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string UserId { get; set; } = "";
    public string? FullName { get; set; }
    public string[]? Roles { get; set; }
}

public class SupabaseResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
}

public class SupabaseUser
{
    public string Id { get; set; } = "";
    public string Email { get; set; } = "";
    public string? FullName { get; set; }
    public string[]? Roles { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}
