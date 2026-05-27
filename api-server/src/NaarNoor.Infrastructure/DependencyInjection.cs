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
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        return services;
    }

    private static string BuildConnectionString(IConfiguration configuration)
    {
        // Try individual PG* env vars first (most reliable in Replit environment)
        var pgHost = Environment.GetEnvironmentVariable("PGHOST");
        var pgPort = Environment.GetEnvironmentVariable("PGPORT") ?? "5432";
        var pgDatabase = Environment.GetEnvironmentVariable("PGDATABASE");
        var pgUser = Environment.GetEnvironmentVariable("PGUSER");
        var pgPassword = Environment.GetEnvironmentVariable("PGPASSWORD");

        if (!string.IsNullOrWhiteSpace(pgHost) && !string.IsNullOrWhiteSpace(pgDatabase))
        {
            // Resolve hostname to IP to avoid .NET DNS issues in Replit
            var resolvedHost = ResolveHost(pgHost);
            return $"Host={resolvedHost};Port={pgPort};Database={pgDatabase};Username={pgUser};Password={pgPassword};Trust Server Certificate=true;SSL Mode=Disable;";
        }

        // Fall back to connection string
        var raw = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(raw))
            raw = configuration["DATABASE_URL"];
        if (string.IsNullOrWhiteSpace(raw))
            throw new InvalidOperationException("Database connection string not found. Set DATABASE_URL or PG* environment variables.");

        return NormaliseConnectionString(raw);
    }

    private static string ResolveHost(string hostname)
    {
        try
        {
            var addresses = System.Net.Dns.GetHostAddresses(hostname);
            if (addresses.Length > 0)
                return addresses[0].ToString();
        }
        catch
        {
            // Fall through to return original hostname
        }
        return hostname;
    }

    private static string NormaliseConnectionString(string value)
    {
        if (!value.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase) &&
            !value.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase))
            return value;

        var uri = new Uri(value);
        var userInfo = uri.UserInfo.Split(':', 2);
        var user = Uri.UnescapeDataString(userInfo[0]);
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
        var host = ResolveHost(uri.Host);
        var port = uri.IsDefaultPort ? 5432 : uri.Port;
        var database = uri.AbsolutePath.TrimStart('/');

        return $"Host={host};Port={port};Database={database};Username={user};Password={password};Trust Server Certificate=true;SSL Mode=Disable;";
    }
}
