namespace NaarNoor.API.Configuration;

/// <summary>
/// CORS service registration
/// </summary>
public static class CorsServiceConfiguration
{
    public static void AddCorsServiceConfiguration(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });
    }
}
