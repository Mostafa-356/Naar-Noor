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

                // Sanitize user-controlled values before logging to prevent log injection
                var safeUserId = SanitizeForLog(userId);
                var safePath   = SanitizeForLog(request.Path.Value ?? "");
                var safeBody   = SanitizeForLog(bodyContent[..Math.Min(500, bodyContent.Length)]);
                // HTTP method is also user-controlled in raw socket connections
                var safeMethod = SanitizeForLog(request.Method);

                // Log audit event
                logger.LogInformation(
                    "AUDIT: {Method} {Path} by {UserId} at {Timestamp} | Body: {Body}",
                    safeMethod,
                    safePath,
                    safeUserId,
                    DateTime.UtcNow,
                    safeBody
                );
            }

            await next();
        });
    }

    /// <summary>
    /// Strips newlines and control characters from a user-supplied string
    /// to prevent log injection attacks.
    /// </summary>
    private static string SanitizeForLog(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        // Replace newlines and carriage returns with a safe placeholder
        return input
            .Replace("\r", " ")
            .Replace("\n", " ")
            .Replace("\t", " ");
    }
}
