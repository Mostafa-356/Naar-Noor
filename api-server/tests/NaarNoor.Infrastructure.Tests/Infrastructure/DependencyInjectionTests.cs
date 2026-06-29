using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NaarNoor.Application.Common.Interfaces;
using NaarNoor.Infrastructure.Data;
using NaarNoor.Infrastructure.Repositories;
using Xunit;

namespace NaarNoor.Infrastructure.Tests.Infrastructure;

public class DependencyInjectionTests
{
    private static IConfiguration BuildConfig(
        string? connectionString = null,
        string? supabaseUrl = null,
        string? supabaseAnonKey = null)
    {
        var dict = new Dictionary<string, string?>();
        if (connectionString is not null)
            dict["ConnectionStrings:DefaultConnection"] = connectionString;
        if (supabaseUrl is not null)
            dict["Supabase:Url"] = supabaseUrl;
        if (supabaseAnonKey is not null)
            dict["Supabase:AnonKey"] = supabaseAnonKey;

        return new ConfigurationBuilder()
            .AddInMemoryCollection(dict)
            .Build();
    }

    private static void ClearDbEnvVars()
    {
        Environment.SetEnvironmentVariable("PGHOST", null);
        Environment.SetEnvironmentVariable("PGPORT", null);
        Environment.SetEnvironmentVariable("PGUSER", null);
        Environment.SetEnvironmentVariable("PGPASSWORD", null);
        Environment.SetEnvironmentVariable("PGDATABASE", null);
        Environment.SetEnvironmentVariable("POSTGRESQL_CONNECTION_STRING", null);
    }

    [Fact]
    public void AddInfrastructure_WithPgEnvVarsAndSupabase_RegistersCoreServices()
    {
        // Arrange — use PG env vars for DB, and Supabase env vars
        Environment.SetEnvironmentVariable("PGHOST", "localhost");
        Environment.SetEnvironmentVariable("PGPORT", "5432");
        Environment.SetEnvironmentVariable("PGUSER", "testuser");
        Environment.SetEnvironmentVariable("PGPASSWORD", "testpass");
        Environment.SetEnvironmentVariable("PGDATABASE", "testdb");
        Environment.SetEnvironmentVariable("SUPABASE_URL", "https://test.supabase.co");
        Environment.SetEnvironmentVariable("SUPABASE_ANON_KEY", "test-anon-key");

        try
        {
            var services = new ServiceCollection();
            var config = BuildConfig();

            services.AddLogging();
            services.AddInfrastructure(config);

            var provider = services.BuildServiceProvider();

            // Verify core services are registered
            provider.GetService<IUnitOfWork>().Should().NotBeNull();
            provider.GetService<IApplicationDbContext>().Should().NotBeNull();
        }
        finally
        {
            ClearDbEnvVars();
            Environment.SetEnvironmentVariable("SUPABASE_URL", null);
            Environment.SetEnvironmentVariable("SUPABASE_ANON_KEY", null);
        }
    }

    [Fact]
    public void AddInfrastructure_WithConnectionStringEnvVar_RegistersDbContext()
    {
        Environment.SetEnvironmentVariable("POSTGRESQL_CONNECTION_STRING", "Host=localhost;Database=test;Username=user;Password=pass;SSL Mode=Disable;");
        Environment.SetEnvironmentVariable("PGHOST", null);
        Environment.SetEnvironmentVariable("PGUSER", null);
        Environment.SetEnvironmentVariable("SUPABASE_URL", "https://test.supabase.co");
        Environment.SetEnvironmentVariable("SUPABASE_ANON_KEY", "test-anon-key");

        try
        {
            var services = new ServiceCollection();
            var config = BuildConfig();

            services.AddLogging();
            services.AddInfrastructure(config);

            var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ApplicationDbContext));
            descriptor.Should().NotBeNull("ApplicationDbContext should be registered");
        }
        finally
        {
            Environment.SetEnvironmentVariable("POSTGRESQL_CONNECTION_STRING", null);
            Environment.SetEnvironmentVariable("SUPABASE_URL", null);
            Environment.SetEnvironmentVariable("SUPABASE_ANON_KEY", null);
        }
    }

    [Fact]
    public void AddInfrastructure_WithConfigConnectionString_RegistersDbContext()
    {
        ClearDbEnvVars();
        Environment.SetEnvironmentVariable("SUPABASE_URL", "https://test.supabase.co");
        Environment.SetEnvironmentVariable("SUPABASE_ANON_KEY", "test-anon-key");

        try
        {
            var services = new ServiceCollection();
            var config = BuildConfig(
                connectionString: "Host=localhost;Database=test;Username=user;Password=real_pass;");

            services.AddLogging();
            services.AddInfrastructure(config);

            var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ApplicationDbContext));
            descriptor.Should().NotBeNull();
        }
        finally
        {
            Environment.SetEnvironmentVariable("SUPABASE_URL", null);
            Environment.SetEnvironmentVariable("SUPABASE_ANON_KEY", null);
        }
    }

    [Fact]
    public void AddInfrastructure_WithNoDatabaseConfig_ThrowsInvalidOperationException()
    {
        ClearDbEnvVars();
        Environment.SetEnvironmentVariable("SUPABASE_URL", null);
        Environment.SetEnvironmentVariable("SUPABASE_ANON_KEY", null);

        var services = new ServiceCollection();
        var config = BuildConfig(); // no connection string in config either

        Action act = () => services.AddInfrastructure(config);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Database connection string not found*");
    }

    [Fact]
    public void AddInfrastructure_WithMissingSupabaseUrl_ThrowsInvalidOperationException()
    {
        Environment.SetEnvironmentVariable("PGHOST", "localhost");
        Environment.SetEnvironmentVariable("PGUSER", "testuser");
        Environment.SetEnvironmentVariable("SUPABASE_URL", null);
        Environment.SetEnvironmentVariable("SUPABASE_ANON_KEY", null);

        try
        {
            var services = new ServiceCollection();
            var config = BuildConfig(); // no Supabase config

            Action act = () => services.AddInfrastructure(config);

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*Supabase URL not configured*");
        }
        finally
        {
            ClearDbEnvVars();
        }
    }

    [Fact]
    public void AddInfrastructure_WithMissingSupabaseAnonKey_ThrowsInvalidOperationException()
    {
        Environment.SetEnvironmentVariable("PGHOST", "localhost");
        Environment.SetEnvironmentVariable("PGUSER", "testuser");
        Environment.SetEnvironmentVariable("SUPABASE_URL", "https://test.supabase.co");
        Environment.SetEnvironmentVariable("SUPABASE_ANON_KEY", null);

        try
        {
            var services = new ServiceCollection();
            var config = BuildConfig(supabaseUrl: null); // no anon key in config

            Action act = () => services.AddInfrastructure(config);

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*Anonymous Key*");
        }
        finally
        {
            ClearDbEnvVars();
            Environment.SetEnvironmentVariable("SUPABASE_URL", null);
        }
    }

    [Fact]
    public void AddInfrastructure_WithConnectionStringContainingPlaceholder_ThrowsInvalidOperationException()
    {
        ClearDbEnvVars();
        Environment.SetEnvironmentVariable("SUPABASE_URL", null);
        Environment.SetEnvironmentVariable("SUPABASE_ANON_KEY", null);

        var services = new ServiceCollection();
        var config = BuildConfig(
            connectionString: "Host=localhost;Database=test;Username=user;Password=YOUR_PASSWORD_HERE;");

        Action act = () => services.AddInfrastructure(config);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Database connection string not found*");
    }

    [Fact]
    public void AddInfrastructure_WithPgEnvVars_RegistersIStripeService()
    {
        Environment.SetEnvironmentVariable("PGHOST", "localhost");
        Environment.SetEnvironmentVariable("PGUSER", "testuser");
        Environment.SetEnvironmentVariable("PGPASSWORD", "testpass");
        Environment.SetEnvironmentVariable("PGDATABASE", "testdb");
        Environment.SetEnvironmentVariable("SUPABASE_URL", "https://test.supabase.co");
        Environment.SetEnvironmentVariable("SUPABASE_ANON_KEY", "test-anon-key");
        Environment.SetEnvironmentVariable("STRIPE_SECRET_KEY", "sk_test_fake_key_for_di_test");

        try
        {
            var services = new ServiceCollection();
            var config = BuildConfig();

            services.AddLogging();
            services.AddInfrastructure(config);

            var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IStripeService));
            descriptor.Should().NotBeNull("IStripeService should be registered as singleton");
        }
        finally
        {
            ClearDbEnvVars();
            Environment.SetEnvironmentVariable("SUPABASE_URL", null);
            Environment.SetEnvironmentVariable("SUPABASE_ANON_KEY", null);
            Environment.SetEnvironmentVariable("STRIPE_SECRET_KEY", null);
        }
    }

    [Fact]
    public void AddInfrastructure_WithPgEnvVarsNoPort_UsesDefaultPort()
    {
        Environment.SetEnvironmentVariable("PGHOST", "localhost");
        Environment.SetEnvironmentVariable("PGPORT", null); // rely on default
        Environment.SetEnvironmentVariable("PGUSER", "testuser");
        Environment.SetEnvironmentVariable("PGPASSWORD", "testpass");
        Environment.SetEnvironmentVariable("PGDATABASE", "testdb");
        Environment.SetEnvironmentVariable("SUPABASE_URL", "https://test.supabase.co");
        Environment.SetEnvironmentVariable("SUPABASE_ANON_KEY", "test-anon-key");

        try
        {
            var services = new ServiceCollection();
            var config = BuildConfig();

            services.AddLogging();

            // Should not throw — default port is 5432
            Action act = () => services.AddInfrastructure(config);
            act.Should().NotThrow();
        }
        finally
        {
            ClearDbEnvVars();
            Environment.SetEnvironmentVariable("SUPABASE_URL", null);
            Environment.SetEnvironmentVariable("SUPABASE_ANON_KEY", null);
        }
    }

    [Fact]
    public void SupabaseConfig_Properties_AreSettable()
    {
        var config = new SupabaseConfig
        {
            Url = "https://test.supabase.co",
            AnonKey = "test-anon-key",
            ServiceRoleKey = "test-service-role"
        };

        config.Url.Should().Be("https://test.supabase.co");
        config.AnonKey.Should().Be("test-anon-key");
        config.ServiceRoleKey.Should().Be("test-service-role");
    }

    [Fact]
    public void SupabaseConfig_DefaultValues_AreEmptyStrings()
    {
        var config = new SupabaseConfig();

        config.Url.Should().Be("");
        config.AnonKey.Should().Be("");
        config.ServiceRoleKey.Should().Be("");
    }
}
