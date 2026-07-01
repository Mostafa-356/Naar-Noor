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
            context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains; preload";
            // ⚠️ SECURITY: Remove 'unsafe-inline' to prevent XSS attacks
            // Use strict CSP with only trusted sources
            context.Response.Headers["Content-Security-Policy"] =
                "default-src 'self'; " +
                "style-src 'self' https://fonts.googleapis.com; " +
                "font-src 'self' https://fonts.gstatic.com; " +
                "script-src 'self'; " +
                "connect-src 'self'; " +
                "img-src 'self' data: https:; " +
                "frame-ancestors 'none'; " +
                "upgrade-insecure-requests; " +
                "base-uri 'self'; " +
                "form-action 'self';";
            context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=(), payment=()";

            await next();
        });
    }
}
