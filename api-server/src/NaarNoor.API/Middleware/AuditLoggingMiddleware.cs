namespace NaarNoor.API.Middleware;

using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using System.Text.Json;

/// <summary>
/// Audit logging middleware for tracking sensitive operations
/// Logs: Create, Update, Delete operations with user ID, timestamp, changes
/// </summary>
public static class AuditLoggingMiddleware
{
    public static void UseAuditLoggingMiddleware(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            var request = context.Request;

            // Only audit sensitive operations
            var auditableMethods = new[] { "POST", "PUT", "DELETE", "PATCH" };
            var auditableEndpoints = new[] 
            { 
                "/api/reservations",
                "/api/orders",
                "/api/reviews",
                "/api/menu",
                "/api/auth",
                "/api/contact"
            };

            var shouldAudit = auditableMethods.Contains(request.Method) &&
                             auditableEndpoints.Any(ep => request.Path.Value?.Contains(ep) ?? false);

            if (shouldAudit)
            {
                // Get request body for audit trail
                var originalBodyStream = request.Body;
                var memoryStream = new MemoryStream();
                await request.Body.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
                request.Body = memoryStream;

                var bodyContent = await new StreamReader(memoryStream).ReadToEndAsync();
                memoryStream.Position = 0;

                // Extract user ID from claims
                var userId = context.User?.FindFirst("sub")?.Value ?? "anonymous";
                
                // Log audit event
                logger.LogInformation(
                    "AUDIT: {Method} {Path} by {UserId} at {Timestamp} | Body: {Body}",
                    request.Method,
                    request.Path,
                    userId,
                    DateTime.UtcNow,
                    bodyContent[..Math.Min(500, bodyContent.Length)]  // Truncate to 500 chars
                );
            }

            await next();
        });
    }
}
