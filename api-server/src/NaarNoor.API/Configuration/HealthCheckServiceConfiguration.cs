using NaarNoor.Infrastructure.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace NaarNoor.API.Configuration;

/// <summary>
/// Health check service registration - Monitors critical application dependencies
/// </summary>
public static class HealthCheckServiceConfiguration
{
    public static void AddHealthCheckServiceConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        // ✅ OBSERVABILITY: Register health checks for all critical dependencies
        services.AddHealthChecks()
            // 1. Database connectivity check
            .AddDbContextCheck<ApplicationDbContext>(
                name: "Database",
                failureStatus: HealthStatus.Degraded,
                tags: new[] { "database", "required" })
            
            // 2. Memory check
            .AddCheck(
                name: "Memory",
                check: () =>
                {
                    var totalMemory = GC.GetTotalMemory(false);
                    var threshold = 500_000_000; // 500 MB threshold
                    return totalMemory < threshold
                        ? HealthCheckResult.Healthy($"Memory usage: {totalMemory / 1_000_000} MB")
                        : HealthCheckResult.Unhealthy($"Memory usage: {totalMemory / 1_000_000} MB (exceeds threshold)");
                },
                tags: new[] { "system", "memory" })
            
            // 3. Disk space check
            .AddCheck(
                name: "DiskSpace",
                check: () =>
                {
                    var drive = new System.IO.DriveInfo(AppDomain.CurrentDomain.BaseDirectory);
                    var minFreeSpaceBytes = 100_000_000; // 100 MB minimum
                    return drive.AvailableFreeSpace > minFreeSpaceBytes
                        ? HealthCheckResult.Healthy($"Free disk space: {drive.AvailableFreeSpace / 1_000_000} MB")
                        : HealthCheckResult.Unhealthy($"Low disk space: {drive.AvailableFreeSpace / 1_000_000} MB");
                },
                tags: new[] { "system", "disk" })
            
            // 4. Supabase connectivity check
            .AddUrlGroup(
                uris: new Uri[] { new Uri("https://api.supabase.co/") },
                name: "Supabase",
                failureStatus: HealthStatus.Degraded,
                tags: new[] { "external", "supabase" })
            
            // 5. Stripe connectivity check (optional, external service)
            .AddUrlGroup(
                uris: new Uri[] { new Uri("https://api.stripe.com/") },
                name: "Stripe",
                failureStatus: HealthStatus.Degraded,
                tags: new[] { "external", "payment" });
    }
}

