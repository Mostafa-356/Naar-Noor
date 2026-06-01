using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NaarNoor.Application.Common.Interfaces;
using NaarNoor.Infrastructure.Data;

namespace NaarNoor.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = BuildConnectionString(configuration);

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString, sqlServer =>
                sqlServer.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        return services;
    }

    private static string BuildConnectionString(IConfiguration configuration)
    {
        // Try environment variable first
        var envConnectionString = Environment.GetEnvironmentVariable("SQLSERVER_CONNECTION_STRING");
        if (!string.IsNullOrWhiteSpace(envConnectionString))
            return envConnectionString;

        // Fall back to configuration
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Database connection string not found. Set SQLSERVER_CONNECTION_STRING environment variable or configure DefaultConnection in appsettings.");

        return connectionString;
    }
}
