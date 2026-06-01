namespace NaarNoor.API.Middleware;

/// <summary>
/// Swagger UI middleware
/// </summary>
public static class SwaggerMiddleware
{
    public static void UseSwaggerMiddleware(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Naar & Noor API v1");
            c.RoutePrefix = "api/docs";
        });
    }
}
