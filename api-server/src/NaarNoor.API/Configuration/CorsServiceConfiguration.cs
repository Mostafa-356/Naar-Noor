namespace NaarNoor.API.Configuration;

using Microsoft.Extensions.Configuration;

/// <summary>
/// CORS service registration - Environment-aware configuration
/// Development: AllowAnyOrigin (localhost development)
/// Production: Specific origins only (CORS_ALLOWED_ORIGINS env var)
/// </summary>
public static class CorsServiceConfiguration
{
    public static void AddCorsServiceConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var environment = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development";
        
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                if (environment == "Development")
                {
                    // Development: Allow localhost for frontend development
                    policy.WithOrigins("http://localhost:5000", "http://localhost:4200", "http://127.0.0.1:5000")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                }
                else
                {
                    // Production: Restrict to configured origins only
                    var allowedOrigins = configuration["CORS:AllowedOrigins"]?.Split(',') 
                        ?? new[] { "https://naar-noor.vercel.app" };
                    
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                }
            });
        });
    }
}
