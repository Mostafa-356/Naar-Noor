using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Builder;
using NaarNoor.API.Configuration;
using NaarNoor.API.Middleware;
using NaarNoor.Application;
using NaarNoor.Infrastructure;
using Serilog;
using Serilog.Formatting.Compact;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog - Phase 2.3: Structured Logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithMachineName()
    .WriteTo.Console(new CompactJsonFormatter())
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Starting Naar-Noor API...");
    Log.Information("Environment: {Environment}", builder.Environment.EnvironmentName);
    Log.Information("Machine: {MachineName}", Environment.MachineName);
    Log.Information("Time: {UtcNow:O}", DateTime.UtcNow);

    // ===== SERVICE REGISTRATION =====

    // 1. Web Host Configuration
    builder.ConfigureWebHost();

    // 2. Core Services
    builder.Services.AddServiceConfiguration();

    // 3. Swagger Services
    builder.Services.AddSwaggerServiceConfiguration();

    // 4. CORS Services - Phase 2.4
    builder.Services.AddCorsServiceConfiguration(builder.Configuration);

    // 5. Health Check Services
    builder.Services.AddHealthCheckServiceConfiguration(builder.Configuration);

    // 6. Application Layer
    builder.Services.AddApplication();

    // 7. Infrastructure Layer (includes Rate Limiting - Phase 2.2)
    builder.Services.AddInfrastructure(builder.Configuration);

    var app = builder.Build();

    // ===== MIDDLEWARE PIPELINE =====

    // 1. Exception Handling (must be first)
    app.UseExceptionHandlingMiddleware();

    // 2. Security Headers
    app.UseSecurityHeadersMiddleware();

    // 3. Rate Limiting - Phase 2.2
    app.UseIpRateLimiting();

    // 4. Static files (wwwroot) — serve CSS, JS and other assets
    app.UseStaticFiles();

    // 5. Swagger UI
    app.UseSwaggerMiddleware();

    // 6. CORS - Phase 2.4
    app.UseCorsMiddleware();

    // 7. ✅ Authentication (MUST be before Authorization)
    app.UseAuthentication();

    // 8. Authorization
    app.UseAuthorizationMiddleware();

    // 9. Map Controllers
    app.MapControllersMiddleware();

    // 9. Map Health Checks
    app.MapHealthChecks("/health");

    // ✅ OBSERVABILITY: Map detailed health checks endpoint with UI
    app.MapHealthChecks("/health/detailed", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";
            using var writer = new System.IO.StreamWriter(context.Response.Body);
            await writer.WriteLineAsync(System.Text.Json.JsonSerializer.Serialize(new
            {
                status = report.Status.ToString(),
                timestamp = DateTime.UtcNow,
                checks = report.Entries.Select(entry => new
                {
                    name = entry.Key,
                    status = entry.Value.Status.ToString(),
                    duration = entry.Value.Duration,
                    description = entry.Value.Description,
                    exception = entry.Value.Exception?.Message
                })
            }));
            await writer.FlushAsync();
        }
    });

    // 10. Explicit HTML page routes
    var webRootPath = app.Environment.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot");

    static RequestDelegate ServePage(string filePath) =>
        async ctx =>
        {
            ctx.Response.ContentType = "text/html; charset=utf-8";
            await ctx.Response.SendFileAsync(filePath);
        };

    app.MapGet("/",       ServePage(Path.Combine(webRootPath, "index.html")));
    app.MapGet("/about",  ServePage(Path.Combine(webRootPath, "about.html")));
    app.MapGet("/status", ServePage(Path.Combine(webRootPath, "status.html")));

    // 11. Custom 404 fallback for all other unknown paths
    app.MapFallback(async context =>
    {
        context.Response.StatusCode = 404;
        context.Response.ContentType = "text/html; charset=utf-8";
        var notFoundPage = Path.Combine(webRootPath, "404.html");
        if (File.Exists(notFoundPage))
            await context.Response.SendFileAsync(notFoundPage);
    });

    // 9. Seed Database
    await app.SeedDatabaseMiddlewareAsync();

    Log.Information("Naar-Noor API started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Exposed as public so WebApplicationFactory<Program> can be used in integration tests.
public partial class Program { }
