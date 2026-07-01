using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace NaarNoor.Infrastructure.Services;

/// <summary>
/// Supabase authentication service implementation
/// Uses Supabase REST API for user authentication
/// </summary>
public class SupabaseService : ISupabaseService
{
    private readonly HttpClient _httpClient;
    private readonly SupabaseConfig _config;
    private readonly ILogger<SupabaseService> _logger;

    public SupabaseService(HttpClient httpClient, SupabaseConfig config, ILogger<SupabaseService> logger)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = logger;
    }

    public async Task<SupabaseAuthResult> SignInAsync(string email, string password)
    {
        try
        {
            // Supabase Auth REST API endpoint
            var url = $"{_config.Url}/auth/v1/token?grant_type=password";
            
            var request = new { email, password };
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Set headers with API key
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("apikey", _config.AnonKey);

            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Supabase sign in failed: {StatusCode} {Error}", response.StatusCode, errorContent);
                return new SupabaseAuthResult { Success = false, Error = "Invalid email or password" };
            }

            var responseData = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseData);
            var root = doc.RootElement;

            var userId = root.GetProperty("user").GetProperty("id").GetString() ?? "";
            var userMetadata = root.GetProperty("user").TryGetProperty("user_metadata", out var metadata)
                ? metadata
                : new JsonElement();

            return new SupabaseAuthResult
            {
                Success = true,
                UserId = userId,
                FullName = userMetadata.TryGetProperty("full_name", out var fullNameElement)
                    ? fullNameElement.GetString()
                    : "",
                Roles = new[] { "User" }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sign in error");
            return new SupabaseAuthResult { Success = false, Error = "Authentication failed" };
        }
    }

    public async Task<SupabaseAuthResult> SignUpAsync(string email, string password, string fullName)
    {
        try
        {
            var url = $"{_config.Url}/auth/v1/signup";

            var request = new
            {
                email,
                password,
                user_metadata = new { full_name = fullName }
            };

            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("apikey", _config.AnonKey);

            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Supabase sign up failed: {StatusCode} {Error}", response.StatusCode, errorContent);
                
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    return new SupabaseAuthResult { Success = false, Error = "Email already exists or invalid" };

                return new SupabaseAuthResult { Success = false, Error = "Registration failed" };
            }

            var responseData = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseData);
            var root = doc.RootElement;

            var userId = root.GetProperty("user").GetProperty("id").GetString() ?? "";

            return new SupabaseAuthResult
            {
                Success = true,
                UserId = userId,
                FullName = fullName,
                Roles = new[] { "User" }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sign up error");
            return new SupabaseAuthResult { Success = false, Error = "Registration failed" };
        }
    }

    public async Task<SupabaseUser?> GetUserAsync(string userId)
    {
        try
        {
            var url = $"{_config.Url}/rest/v1/rpc/get_user_by_id?user_id={userId}";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("apikey", _config.ServiceRoleKey);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.ServiceRoleKey}");

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get user {UserId}", userId);
                return null;
            }

            var responseData = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseData);
            
            // Assuming response is an array with user data
            if (doc.RootElement.ValueKind == JsonValueKind.Array && doc.RootElement.GetArrayLength() > 0)
            {
                var userData = doc.RootElement[0];
                return new SupabaseUser
                {
                    Id = userData.GetProperty("id").GetString() ?? "",
                    Email = userData.GetProperty("email").GetString() ?? "",
                    FullName = userData.TryGetProperty("full_name", out var fullName) 
                        ? fullName.GetString() 
                        : null,
                    Roles = new[] { "User" },
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user {UserId}", userId);
            return null;
        }
    }

    public async Task<SupabaseUser?> GetUserByEmailAsync(string email)
    {
        try
        {
            var url = $"{_config.Url}/rest/v1/rpc/get_user_by_email?user_email={Uri.EscapeDataString(email)}";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("apikey", _config.ServiceRoleKey);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.ServiceRoleKey}");

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return null;

            var responseData = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseData);

            if (doc.RootElement.ValueKind == JsonValueKind.Array && doc.RootElement.GetArrayLength() > 0)
            {
                var userData = doc.RootElement[0];
                return new SupabaseUser
                {
                    Id = userData.GetProperty("id").GetString() ?? "",
                    Email = userData.GetProperty("email").GetString() ?? "",
                    FullName = userData.TryGetProperty("full_name", out var fullName)
                        ? fullName.GetString()
                        : null,
                    Roles = new[] { "User" },
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user by email {Email}", email);
            return null;
        }
    }

    public async Task<SupabaseResult> UpdatePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        try
        {
            // First, verify current password by attempting sign in
            var verifyResult = await SignInAsync("", currentPassword);
            if (!verifyResult.Success)
                return new SupabaseResult { Success = false, Error = "Current password is incorrect" };

            // Use service role to update password
            var url = $"{_config.Url}/auth/v1/admin/users/{userId}";
            var request = new { password = newPassword };
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("apikey", _config.ServiceRoleKey);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.ServiceRoleKey}");

            var response = await _httpClient.PutAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to update password for user {UserId}", userId);
                return new SupabaseResult { Success = false, Error = "Failed to update password" };
            }

            return new SupabaseResult { Success = true };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Password update failed for user {UserId}", userId);
            return new SupabaseResult { Success = false, Error = "Password update failed" };
        }
    }

    public async Task<SupabaseResult> VerifyEmailAsync(string userId)
    {
        try
        {
            var url = $"{_config.Url}/rest/v1/rpc/verify_email?user_id={userId}";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("apikey", _config.ServiceRoleKey);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.ServiceRoleKey}");

            var response = await _httpClient.PostAsync(url, null);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to verify email for user {UserId}", userId);
                return new SupabaseResult { Success = false, Error = "Email verification failed" };
            }

            return new SupabaseResult { Success = true };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email verification failed for user {UserId}", userId);
            return new SupabaseResult { Success = false, Error = "Email verification failed" };
        }
    }
}
