using AspNetCoreRateLimit;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NaarNoor.Application.Caching;
using NaarNoor.Application.Common.Interfaces;
using NaarNoor.Application.Services;
using NaarNoor.Infrastructure.Data;
using NaarNoor.Infrastructure.Repositories;
using NaarNoor.Infrastructure.Services;
using System.Text;

namespace NaarNoor.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = BuildConnectionString(configuration);

        // Database Context - PostgreSQL via Npgsql
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // ✅ JWT AUTHENTICATION
        var jwtSecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
            ?? configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("JWT_SECRET_KEY not configured");

        var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
            ?? configuration["Jwt:Issuer"]
            ?? "NaarNoor";

        var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
            ?? configuration["Jwt:Audience"]
            ?? "NaarNoorApp";

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,
                ValidateAudience = true,
                ValidAudience = jwtAudience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5),
                RequireExpirationTime = true
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                    {
                        context.Response.Headers.Add("Token-Expired", "true");
                    }
                    return Task.CompletedTask;
                }
            };
        });

        // ✅ AUTHORIZATION
        services.AddAuthorization();

        // ✅ JWT SERVICE
        services.AddScoped<IJwtService, JwtService>();

        // ✅ USER SERVICE (Authentication & User Management)
        services.AddScoped<IUserService, UserService>();

        // Rate Limiting (via built-in AspNetCoreRateLimit)
        services.AddMemoryCache();
        
        // ✅ PERFORMANCE: Distributed caching with Redis (optional, falls back to memory)
        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redisConnection))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = "NaarNoor_";
            });
        }
        else
        {
            // Fallback to memory cache for local development
            services.AddDistributedMemoryCache();
        }
        
        // ✅ Add cache service for application layer
        services.AddScoped<ICacheService, DistributedCacheService>();
        
        services.Configure<IpRateLimitOptions>(options =>
        {
            options.EnableEndpointRateLimiting = true;
            options.StackBlockedRequests = false;
            options.RealIpHeader = "X-Real-IP";
            options.HttpStatusCode = 429;
            options.GeneralRules = new List<RateLimitRule>
            {
                new() { Endpoint = "*/auth/register", Period = "1m", Limit = 5 },
                new() { Endpoint = "*/auth/login", Period = "1m", Limit = 10 },
                new() { Endpoint = "*/contact", Period = "1h", Limit = 10 },
                new() { Endpoint = "*", Period = "1m", Limit = 100 },
            };
        });
        services.AddInMemoryRateLimiting();
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

        // Supabase Services - REST API based implementation
        var supabaseUrl = Environment.GetEnvironmentVariable("SUPABASE_URL")
            ?? configuration["Supabase:Url"]
            ?? throw new InvalidOperationException("Supabase URL not configured");
        var supabaseAnonKey = Environment.GetEnvironmentVariable("SUPABASE_ANON_KEY")
            ?? configuration["Supabase:AnonKey"]
            ?? throw new InvalidOperationException("Supabase Anonymous Key not configured");
        
        services.AddHttpClient<ISupabaseAuthService, SupabaseAuthService>();
        services.AddHttpClient<ISupabaseStorageService, SupabaseStorageService>();
        services.AddHttpClient<ISupabaseRealtimeService, SupabaseRealtimeService>();
        services.AddHttpClient<ISupabaseService, SupabaseService>();

        // Stripe Payment Service
        services.AddSingleton<IStripeService, StripeService>();

        var serviceRoleKey = Environment.GetEnvironmentVariable("SUPABASE_SERVICE_ROLE_KEY")
            ?? configuration["Supabase:ServiceRoleKey"]
            ?? "";

        services.AddSingleton(new SupabaseConfig 
        { 
            Url = supabaseUrl, 
            AnonKey = supabaseAnonKey,
            ServiceRoleKey = serviceRoleKey
        });

        // ✅ APM: Application Insights for monitoring (Phase 2)
        var appInsightsKey = Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")
            ?? configuration["ApplicationInsights:InstrumentationKey"];
        
        if (!string.IsNullOrWhiteSpace(appInsightsKey))
        {
            services.AddApplicationInsightsTelemetry(options =>
            {
                options.ConnectionString = appInsightsKey;
                options.EnableAdaptiveSampling = true;
                options.EnableHeartbeat = true;
            });
            
            // Track custom metrics
            services.AddSingleton<TelemetryClient>();
        }

        return services;
    }

    private static string BuildConnectionString(IConfiguration configuration)
    {
        // 1. Replit built-in PostgreSQL — build from individual PG* env vars (always reliable)
        var pgHost = Environment.GetEnvironmentVariable("PGHOST");
        var pgPort = Environment.GetEnvironmentVariable("PGPORT") ?? "5432";
        var pgUser = Environment.GetEnvironmentVariable("PGUSER");
        var pgPassword = Environment.GetEnvironmentVariable("PGPASSWORD");
        var pgDatabase = Environment.GetEnvironmentVariable("PGDATABASE");

        if (!string.IsNullOrWhiteSpace(pgHost) && !string.IsNullOrWhiteSpace(pgUser))
            return $"Host={pgHost};Port={pgPort};Database={pgDatabase};Username={pgUser};Password={pgPassword};SSL Mode=Require;";

        // 2. Explicit full connection string override
        var envConnectionString = Environment.GetEnvironmentVariable("POSTGRESQL_CONNECTION_STRING");
        if (!string.IsNullOrWhiteSpace(envConnectionString))
            return envConnectionString;

        // 3. Fall back to appsettings (for external dev environments with real values)
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString) || connectionString.Contains("YOUR_PASSWORD_HERE"))
            throw new InvalidOperationException(
                "Database connection string not found. Set PGHOST/PGUSER/PGPASSWORD/PGDATABASE environment variables.");

        return connectionString;
    }
}

public class SupabaseConfig
{
    public string Url { get; set; } = "";
    public string AnonKey { get; set; } = "";
    public string ServiceRoleKey { get; set; } = "";
}
