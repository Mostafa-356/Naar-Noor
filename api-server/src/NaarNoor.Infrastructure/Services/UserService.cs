using Microsoft.Extensions.Logging;
using NaarNoor.Application.Services;
using NaarNoor.Infrastructure.Data;
using System.Security.Cryptography;
using System.Text;

namespace NaarNoor.Infrastructure.Services;

/// <summary>
/// User service implementation with Supabase integration
/// </summary>
public class UserService : IUserService
{
    private readonly ISupabaseService _supabaseService;
    private readonly ILogger<UserService> _logger;

    public UserService(ISupabaseService supabaseService, ILogger<UserService> logger)
    {
        _supabaseService = supabaseService;
        _logger = logger;
    }

    public async Task<UserAuthResult> AuthenticateAsync(string email, string password)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return new UserAuthResult { Success = false, Error = "Email and password are required" };

            // Use Supabase auth
            var result = await _supabaseService.SignInAsync(email, password);

            if (!result.Success)
                return new UserAuthResult { Success = false, Error = result.Error ?? "Authentication failed" };

            _logger.LogInformation("User {Email} authenticated successfully", email);

            return new UserAuthResult
            {
                Success = true,
                User = new UserDto
                {
                    Id = result.UserId,
                    Email = email,
                    FullName = result.FullName ?? "",
                    Roles = result.Roles ?? new[] { "User" },
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Authentication failed for {Email}", email);
            return new UserAuthResult { Success = false, Error = "Authentication failed" };
        }
    }

    public async Task<UserAuthResult> RegisterAsync(string email, string password, string fullName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
                return new UserAuthResult { Success = false, Error = "Email is required" };

            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                return new UserAuthResult { Success = false, Error = "Password must be at least 8 characters" };

            // Check if user already exists
            var existingUser = await GetUserByEmailAsync(email);
            if (existingUser != null)
                return new UserAuthResult { Success = false, Error = "User already exists" };

            // Create user in Supabase
            var result = await _supabaseService.SignUpAsync(email, password, fullName);

            if (!result.Success)
                return new UserAuthResult { Success = false, Error = result.Error ?? "Registration failed" };

            _logger.LogInformation("User {Email} registered successfully", email);

            return new UserAuthResult
            {
                Success = true,
                User = new UserDto
                {
                    Id = result.UserId,
                    Email = email,
                    FullName = fullName,
                    Roles = new[] { "User" },
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration failed for {Email}", email);
            return new UserAuthResult { Success = false, Error = "Registration failed" };
        }
    }

    public async Task<UserDto?> GetUserByIdAsync(string userId)
    {
        try
        {
            var user = await _supabaseService.GetUserAsync(userId);
            if (user == null)
                return null;

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName ?? "",
                Roles = user.Roles ?? new[] { "User" },
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user {UserId}", userId);
            return null;
        }
    }

    public async Task<UserDto?> GetUserByEmailAsync(string email)
    {
        try
        {
            var user = await _supabaseService.GetUserByEmailAsync(email);
            if (user == null)
                return null;

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName ?? "",
                Roles = user.Roles ?? new[] { "User" },
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user by email {Email}", email);
            return null;
        }
    }

    public async Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
            {
                _logger.LogWarning("Invalid password change attempt for {UserId}", userId);
                return false;
            }

            var result = await _supabaseService.UpdatePasswordAsync(userId, currentPassword, newPassword);
            
            if (result.Success)
                _logger.LogInformation("Password changed for user {UserId}", userId);
            else
                _logger.LogWarning("Password change failed for user {UserId}", userId);

            return result.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Password change failed for {UserId}", userId);
            return false;
        }
    }
}
