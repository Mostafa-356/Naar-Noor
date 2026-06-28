using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NaarNoor.Infrastructure.Data;
using Xunit;

namespace NaarNoor.Infrastructure.Tests.Fixtures;

/// <summary>
/// Base class for repository and database integration tests. Provides database fixture management,
/// async setup/teardown using IAsyncLifetime, and common assertion helpers for data access tests.
/// </summary>
/// <remarks>
/// Validates: Requirements 10.5, 10.6, 11.1
/// 
/// This base class:
/// - Manages database lifecycle with IAsyncLifetime for proper async setup/teardown
/// - Provides factory methods for creating isolated test database contexts
/// - Ensures database is cleaned up after each test execution
/// - Supplies helpers for asserting repository behavior and data persistence
/// - Enables test data seeding and cleanup patterns
/// </remarks>
public abstract class RepositoryTestBase : IAsyncLifetime
{
    private readonly ApplicationDbContextFactory _dbContextFactory;
    protected ApplicationDbContext DbContext { get; private set; } = null!;

    protected RepositoryTestBase()
    {
        _dbContextFactory = new ApplicationDbContextFactory();
    }

    /// <summary>
    /// Called by xUnit before each test to initialize the database context.
    /// </summary>
    public async Task InitializeAsync()
    {
        DbContext = await _dbContextFactory.CreateDbContextAsync();
        
        // Ensure database is empty at start of test
        await DbContext.Database.EnsureDeletedAsync();
        await DbContext.Database.EnsureCreatedAsync();
    }

    /// <summary>
    /// Called by xUnit after each test to dispose of the database context and clean up.
    /// </summary>
    public async Task DisposeAsync()
    {
        await DbContext.Database.EnsureDeletedAsync();
        await DbContext.DisposeAsync();
    }

    /// <summary>
    /// Creates a new isolated database context for use in tests.
    /// Useful when testing multiple database interactions in sequence.
    /// </summary>
    protected async Task<ApplicationDbContext> CreateIsolatedDbContextAsync()
    {
        return await _dbContextFactory.CreateDbContextAsync();
    }

    /// <summary>
    /// Helper to assert that an entity was persisted to the database with expected values.
    /// </summary>
    protected async Task AssertEntityPersistedAsync<TEntity>(
        Guid entityId,
        Action<TEntity> assertions) where TEntity : class
    {
        // Query in a fresh context to ensure we're reading from database
        await using var freshContext = await CreateIsolatedDbContextAsync();
        var entity = await freshContext.Set<TEntity>().FindAsync(entityId);

        entity.Should().NotBeNull("Entity should be persisted to database");
        assertions(entity!);
    }

    /// <summary>
    /// Helper to assert that an entity was deleted from the database.
    /// </summary>
    protected async Task AssertEntityDeletedAsync<TEntity>(Guid entityId) where TEntity : class
    {
        await using var freshContext = await CreateIsolatedDbContextAsync();
        var entity = await freshContext.Set<TEntity>().FindAsync(entityId);
        entity.Should().BeNull("Entity should be deleted from database");
    }

    /// <summary>
    /// Helper to assert that a query returns expected number of results.
    /// </summary>
    protected async Task AssertQueryReturnsCountAsync<TEntity>(
        Func<IQueryable<TEntity>, IQueryable<TEntity>> queryBuilder,
        int expectedCount) where TEntity : class
    {
        var query = queryBuilder(DbContext.Set<TEntity>());
        var count = await query.CountAsync();
        count.Should().Be(expectedCount, $"Query should return {expectedCount} results");
    }

    /// <summary>
    /// Helper to assert that a query returns only matching results.
    /// </summary>
    protected async Task AssertQueryReturnsMatchingAsync<TEntity>(
        Func<IQueryable<TEntity>, IQueryable<TEntity>> queryBuilder,
        Func<TEntity, bool> matchPredicate,
        int? expectedCount = null) where TEntity : class
    {
        var query = queryBuilder(DbContext.Set<TEntity>());
        var results = await query.ToListAsync();

        results.Should().AllSatisfy(entity =>
        {
            matchPredicate(entity).Should().BeTrue($"Result should match predicate");
        });

        if (expectedCount.HasValue)
        {
            results.Count.Should().Be(expectedCount.Value);
        }
    }

    /// <summary>
    /// Helper to save changes to the database and assert success.
    /// </summary>
    protected async Task AssertChangesSavedAsync()
    {
        var result = await DbContext.SaveChangesAsync();
        result.Should().BeGreaterThan(0, "SaveChanges should return count of affected rows");
    }

    /// <summary>
    /// Helper to assert that no changes were made (count is 0).
    /// </summary>
    protected async Task AssertNoChangesSavedAsync()
    {
        var result = await DbContext.SaveChangesAsync();
        result.Should().Be(0, "SaveChanges should return 0 for no changes");
    }

    /// <summary>
    /// Helper to assert that a foreign key constraint is enforced.
    /// </summary>
    protected async Task AssertForeignKeyConstraintEnforcedAsync<TEntity>(
        Func<Task> action) where TEntity : class
    {
        await action.Should().ThrowAsync<DbUpdateException>();
    }

    /// <summary>
    /// Helper to assert that a unique constraint is enforced.
    /// </summary>
    protected async Task AssertUniqueConstraintEnforcedAsync(Func<Task> action)
    {
        await action.Should().ThrowAsync<DbUpdateException>();
    }

    /// <summary>
    /// Helper to seed test data into the database.
    /// </summary>
    protected async Task SeedAsync<TEntity>(params TEntity[] entities) where TEntity : class
    {
        await DbContext.Set<TEntity>().AddRangeAsync(entities);
        await DbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Helper to clear all data of a specific entity type from the database.
    /// </summary>
    protected async Task ClearAsync<TEntity>() where TEntity : class
    {
        var entities = await DbContext.Set<TEntity>().ToListAsync();
        DbContext.Set<TEntity>().RemoveRange(entities);
        await DbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Helper to get a fresh count of entities from the database.
    /// Useful for verifying state changes across test steps.
    /// </summary>
    protected async Task<int> GetCountAsync<TEntity>() where TEntity : class
    {
        return await DbContext.Set<TEntity>().CountAsync();
    }

    /// <summary>
    /// Helper to assert that cascading delete works correctly when a parent entity is deleted.
    /// </summary>
    protected async Task AssertCascadeDeleteAsync<TParent, TChild>(
        TParent parentEntity,
        int expectedChildrenBeforeDeletion,
        int expectedChildrenAfterDeletion = 0) where TParent : class where TChild : class
    {
        // Verify children exist before delete
        var childCountBefore = await DbContext.Set<TChild>().CountAsync();
        childCountBefore.Should().Be(expectedChildrenBeforeDeletion);

        // Delete parent
        DbContext.Set<TParent>().Remove(parentEntity);
        await DbContext.SaveChangesAsync();

        // Verify children were deleted
        await using var freshContext = await CreateIsolatedDbContextAsync();
        var childCountAfter = await freshContext.Set<TChild>().CountAsync();
        childCountAfter.Should().Be(expectedChildrenAfterDeletion);
    }
}
