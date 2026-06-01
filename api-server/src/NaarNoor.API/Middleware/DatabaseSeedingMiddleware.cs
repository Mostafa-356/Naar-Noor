using NaarNoor.Infrastructure.Data;
using NaarNoor.Infrastructure.Data.Seeds;

namespace NaarNoor.API.Middleware;

/// <summary>
/// Database seeding middleware
/// </summary>
public static class DatabaseSeedingMiddleware
{
    public static async Task SeedDatabaseMiddlewareAsync(this WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            try
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await DatabaseSeeder.SeedAsync(db);
            }
            catch (Exception ex)
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "Database seeding failed");
            }
        }
    }
}
