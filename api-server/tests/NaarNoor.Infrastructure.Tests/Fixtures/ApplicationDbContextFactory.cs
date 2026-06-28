using Microsoft.EntityFrameworkCore;
using NaarNoor.Infrastructure.Data;

namespace NaarNoor.Infrastructure.Tests.Fixtures;

/// <summary>
/// Factory for creating isolated in-memory database contexts for testing.
/// Each context gets its own in-memory database instance to ensure test isolation.
/// </summary>
public class ApplicationDbContextFactory
{
    /// <summary>
    /// Creates a new isolated ApplicationDbContext with in-memory database.
    /// </summary>
    public async Task<ApplicationDbContext> CreateDbContextAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // Unique DB for each context
            .Options;

        var context = new ApplicationDbContext(options);
        await context.Database.EnsureCreatedAsync();
        return context;
    }
}
