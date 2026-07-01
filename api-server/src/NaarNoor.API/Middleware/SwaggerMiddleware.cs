namespace NaarNoor.API.Middleware;

using Microsoft.OpenApi.Models;

/// <summary>
/// Swagger UI middleware configuration with API versioning & deprecation policy
/// </summary>
public static class SwaggerMiddleware
{
    public static void UseSwaggerMiddleware(this WebApplication app)
    {
        // ⚠️ SECURITY: Only enable Swagger in Development environment
        if (!app.Environment.IsDevelopment())
            return;
        
        // Enable Swagger JSON endpoint at /swagger/v1/swagger.json
        app.UseSwagger();
        
        // Enable Swagger UI at /api/docs
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Naar & Noor API v1.0.0");
            c.RoutePrefix = "api/docs";
            c.DocumentTitle = "Naar & Noor API Documentation";
        });
    }

    /// <summary>
    /// Configure Swagger generation with versioning and servers
    /// </summary>
    public static void AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Naar & Noor API",
                Version = "1.0.0",
                Description = "Restaurant management system API\n\n**Deprecation Policy:** Endpoints marked [Deprecated] will be removed 6 months after deprecation notice.",
                Contact = new OpenApiContact
                {
                    Name = "Naar & Noor Support",
                    Email = "support@naarnoor.com"
                }
            });

            // ✅ Document API servers
            options.AddServer(new OpenApiServer
            {
                Url = "http://localhost:8080",
                Description = "Development"
            });
            options.AddServer(new OpenApiServer
            {
                Url = "https://api.naar-noor.com",
                Description = "Production"
            });

            // ✅ JWT Bearer authentication scheme
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Enter JWT Bearer token from /api/auth/login"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
    }
}
