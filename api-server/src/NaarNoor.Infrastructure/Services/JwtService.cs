using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace NaarNoor.Infrastructure.Services;

/// <summary>
/// Service for generating and validating JWT tokens
/// </summary>
public interface IJwtService
{
    string GenerateToken(string userId, string email, string[] roles);
}

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtService> _logger;

    public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public string GenerateToken(string userId, string email, string[] roles)
    {
        try
        {
            var secretKey = _configuration["Jwt:SecretKey"]
                ?? throw new InvalidOperationException("Jwt:SecretKey not configured");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId),
                new(ClaimTypes.Email, email),
                new("uid", userId)
            };

            // Add role claims
            if (roles != null && roles.Length > 0)
            {
                foreach (var role in roles)
                {
                    claims.Add(new(ClaimTypes.Role, role));
                }
            }
            else
            {
                // Default role if none provided
                claims.Add(new(ClaimTypes.Role, "User"));
            }

            var expirationMinutes = int.TryParse(_configuration["Jwt:ExpirationMinutes"], out var minutes)
                ? minutes
                : 60;

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"] ?? "NaarNoor",
                audience: _configuration["Jwt:Audience"] ?? "NaarNoorApp",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            _logger.LogInformation("JWT token generated for user {UserId}", userId);
            return tokenString;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating JWT token for user {UserId}", userId);
            throw;
        }
    }
}
