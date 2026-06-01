using NaarNoor.Infrastructure.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace NaarNoor.API.Configuration;

/// <summary>
/// Health check service registration
/// </summary>
public static class HealthCheckServiceConfiguration
{
    public static void AddHealthCheckServiceConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks();
    }
}
