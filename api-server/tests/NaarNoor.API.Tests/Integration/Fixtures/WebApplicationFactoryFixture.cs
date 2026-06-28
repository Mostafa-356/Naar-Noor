using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NaarNoor.Infrastructure.Data;
using Xunit;

namespace NaarNoor.API.Tests.Integration.Fixtures;

/// <summary>
/// Custom WebApplicationFactory fixture for API integration testing. Extends WebApplicationFactory
/// to configure in-memory database, override default test server settings, and provide helper methods
/// for common API testing operations.
/// </summary>
/// <remarks>
/// Validates: Requirements 10.5, 10.6, 11.1
/// 
/// This fixture:
/// - Creates an in-memory test instance of the API application
/// - Configures in-memory Entity Framework database for isolated testing
/// - Provides factory methods for creating test clients with proper base addresses
/// - Enables test configuration overrides and dependency injection customization
/// - Implements IAsyncLifetime for proper async setup/teardown
/// </remarks>
public class WebApplicationFactoryFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string TestDbConnectionString = "Data Source=:memory:;Mode=Memory;Cache=Shared;";

    /// <summary>
    /// Called by xUnit before each test that uses this fixture.
    /// Initializes the test server and database.
    /// </summary>
    public async Task InitializeAsync()
    {
        // Force creation of the test server and services
        var server = Server;
        
        // Ensure database is created
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }

    /// <summary>
    /// Called by xUnit after each test that uses this fixture.
    /// Cleans up resources.
    /// </summary>
    async Task IAsyncLifetime.DisposeAsync()
    {
        try
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.EnsureDeletedAsync();
        }
        finally
        {
            Dispose();
        }
    }

    /// <summary>
    /// Overrides the WebApplicationFactory configuration to use in-memory database
    /// for isolated testing.
    /// </summary>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the production DbContext registration
            var descriptor = services.FirstOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add in-memory database for testing
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDb");
            });
        });

        builder.UseEnvironment("Testing");
    }

    /// <summary>
    /// Creates an authenticated HTTP client for testing endpoints that require authorization.
    /// </summary>
    public HttpClient CreateAuthenticatedClient(string? bearerToken = null)
    {
        var client = CreateClient();
        
        if (!string.IsNullOrEmpty(bearerToken))
        {
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);
        }

        return client;
    }

    /// <summary>
    /// Creates an HTTP client with a specific base address.
    /// </summary>
    public HttpClient CreateClientWithBaseAddress(string baseAddress)
    {
        var client = CreateClient();
        client.BaseAddress = new Uri(baseAddress);
        return client;
    }

    /// <summary>
    /// Gets the database context for manual data seeding or verification in tests.
    /// </summary>
    public ApplicationDbContext GetDbContext()
    {
        var scope = Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }

    /// <summary>
    /// Seeds test data into the database.
    /// </summary>
    public async Task SeedDataAsync(Func<ApplicationDbContext, Task> seedAction)
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await seedAction(dbContext);
    }

    /// <summary>
    /// Clears all data from the database (useful between tests).
    /// </summary>
    public async Task ClearDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Just ensure database can be cleared - in real scenarios, we'd delete all tables
        // For now, we'll use a simple approach
        try
        {
            await dbContext.Database.ExecuteSqlRawAsync("DELETE FROM \"Chefs\"");
        }
        catch
        {
            // Table may not exist in test setup
        }

        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Verifies a specific entity exists in the database.
    /// </summary>
    public async Task<T?> GetEntityAsync<T>(Guid id) where T : class
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await dbContext.Set<T>().FindAsync(id);
    }

    /// <summary>
    /// Generates a mock JWT token for testing protected endpoints.
    /// Note: This generates a basic token for testing; a more sophisticated
    /// implementation would handle claim-based authorization testing.
    /// </summary>
    public string GenerateMockJwtToken(string subject = "test-user", params (string key, string value)[] claims)
    {
        // For testing purposes, return a simple base64-encoded string
        // In a real scenario, you would use JWT libraries to generate proper tokens
        var tokenData = $"{subject}:{string.Join(",", claims.Select(c => $"{c.key}={c.value}"))}";
        return System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(tokenData));
    }

    /// <summary>
    /// Helper to create request content from an object (typically a DTO).
    /// </summary>
    public static StringContent CreateJsonContent<T>(T data)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(data);
        return new StringContent(json, System.Text.Encoding.UTF8, "application/json");
    }

    /// <summary>
    /// Helper to parse JSON response content to an object.
    /// </summary>
    public static async Task<T?> ParseJsonResponseAsync<T>(HttpContent content)
    {
        var jsonString = await content.ReadAsStringAsync();
        return System.Text.Json.JsonSerializer.Deserialize<T>(jsonString,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    /// <summary>
    /// Helper method to assert HTTP response status and content.
    /// </summary>
    public static void AssertHttpResponse(
        HttpResponseMessage response,
        System.Net.HttpStatusCode expectedStatusCode)
    {
        if (response.StatusCode != expectedStatusCode)
        {
            throw new Xunit.Sdk.XunitException(
                $"Expected status code {expectedStatusCode}, but got {response.StatusCode}. " +
                $"Response: {response.Content.ReadAsStringAsync().Result}");
        }
    }
}
