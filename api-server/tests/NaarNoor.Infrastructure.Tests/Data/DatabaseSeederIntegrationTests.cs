using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NaarNoor.Infrastructure.Data;
using NaarNoor.Infrastructure.Data.Seeds;
using Xunit;

namespace NaarNoor.Infrastructure.Tests.Data;

public class DatabaseSeederIntegrationTests : IAsyncLifetime
{
    private ApplicationDbContext _context = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("Seeder_Integration_" + Guid.NewGuid())
            .Options;
        _context = new ApplicationDbContext(options);
        await _context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync() => await _context.DisposeAsync();

    [Fact]
    public async Task SeedDataAsync_WhenDatabaseIsEmpty_SeedsMenuItems()
    {
        await DatabaseSeeder.SeedDataAsync(_context);

        var count = await _context.MenuItems.CountAsync();
        count.Should().BeGreaterThan(0, "SeedDataAsync should seed menu items into an empty database");
    }

    [Fact]
    public async Task SeedDataAsync_WhenDatabaseIsEmpty_SeedsChefs()
    {
        await DatabaseSeeder.SeedDataAsync(_context);

        var count = await _context.Chefs.CountAsync();
        count.Should().BeGreaterThan(0, "SeedDataAsync should seed chefs into an empty database");
    }

    [Fact]
    public async Task SeedDataAsync_WhenDatabaseIsEmpty_SeedsReviews()
    {
        await DatabaseSeeder.SeedDataAsync(_context);

        var count = await _context.Reviews.CountAsync();
        count.Should().BeGreaterThan(0, "SeedDataAsync should seed reviews into an empty database");
    }

    [Fact]
    public async Task SeedDataAsync_SeededMenuItems_HaveValidPrices()
    {
        await DatabaseSeeder.SeedDataAsync(_context);

        var items = await _context.MenuItems.ToListAsync();
        items.Should().OnlyContain(m => m.Price > 0, "All seeded menu items should have a positive price");
    }

    [Fact]
    public async Task SeedDataAsync_SeededMenuItems_HaveNames()
    {
        await DatabaseSeeder.SeedDataAsync(_context);

        var items = await _context.MenuItems.ToListAsync();
        items.Should().OnlyContain(m => !string.IsNullOrWhiteSpace(m.Name), "All seeded menu items should have a name");
    }

    [Fact]
    public async Task SeedDataAsync_SeededChefs_AreActive()
    {
        await DatabaseSeeder.SeedDataAsync(_context);

        var chefs = await _context.Chefs.ToListAsync();
        chefs.Should().OnlyContain(c => c.IsActive, "All seeded chefs should be active");
    }

    [Fact]
    public async Task SeedDataAsync_SeededReviews_AreApproved()
    {
        await DatabaseSeeder.SeedDataAsync(_context);

        var reviews = await _context.Reviews.ToListAsync();
        reviews.Should().OnlyContain(r => r.IsApproved, "All seeded reviews should be approved");
    }

    [Fact]
    public async Task SeedDataAsync_CalledTwice_DoesNotDuplicateData()
    {
        await DatabaseSeeder.SeedDataAsync(_context);
        var countAfterFirst = await _context.MenuItems.CountAsync();

        await DatabaseSeeder.SeedDataAsync(_context);
        var countAfterSecond = await _context.MenuItems.CountAsync();

        countAfterSecond.Should().Be(countAfterFirst, "Calling SeedDataAsync twice should not duplicate data");
    }

    [Fact]
    public async Task SeedDataAsync_SeededReviews_HaveValidRatings()
    {
        await DatabaseSeeder.SeedDataAsync(_context);

        var reviews = await _context.Reviews.ToListAsync();
        reviews.Should().OnlyContain(r => r.Rating >= 1 && r.Rating <= 5,
            "All seeded reviews should have ratings between 1 and 5");
    }

    [Fact]
    public async Task SeedDataAsync_SeededMenuItems_CoverMultipleCategories()
    {
        await DatabaseSeeder.SeedDataAsync(_context);

        var categories = await _context.MenuItems
            .Select(m => m.Category)
            .Distinct()
            .ToListAsync();

        categories.Should().HaveCountGreaterThan(1, "Seeded menu items should span multiple categories");
    }
}
