using Microsoft.OpenApi.Models;

namespace NaarNoor.API.Configuration;

/// <summary>
/// Swagger/OpenAPI service registration
/// </summary>
public static class SwaggerServiceConfiguration
{
    public static void AddSwaggerServiceConfiguration(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Naar & Noor API",
                Version = "v1",
                Description = "Restaurant management API"
            });
        });
    }
}
