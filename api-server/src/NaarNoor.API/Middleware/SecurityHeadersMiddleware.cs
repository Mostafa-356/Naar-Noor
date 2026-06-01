namespace NaarNoor.API.Middleware;

/// <summary>
/// Security headers middleware
/// </summary>
public static class SecurityHeadersMiddleware
{
    public static void UseSecurityHeadersMiddleware(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            context.Response.Headers["X-Frame-Options"] = "DENY";
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";
            context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
            context.Response.Headers["Content-Security-Policy"] = "default-src 'self'";
            context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

            await next();
        });
    }
}
