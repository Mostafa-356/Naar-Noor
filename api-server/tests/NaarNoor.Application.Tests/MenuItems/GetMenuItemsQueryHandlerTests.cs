using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NaarNoor.Application.MenuItems.Queries.GetMenuItems;
using NaarNoor.Domain.Entities;
using NaarNoor.Domain.Enums;
using NaarNoor.Infrastructure.Data;
using NaarNoor.Infrastructure.Repositories;
using Xunit;

namespace NaarNoor.Application.Tests.MenuItems;

public class GetMenuItemsQueryHandlerTests : IAsyncLifetime
{
    private ApplicationDbContext _context = null!;
    private GetMenuItemsQueryHandler _handler = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("GetMenuItems_" + Guid.NewGuid())
            .Options;
        _context = new ApplicationDbContext(options);
        await _context.Database.EnsureCreatedAsync();

        var unitOfWork = new UnitOfWork(_context);
        _handler = new GetMenuItemsQueryHandler(unitOfWork);

        // Seed some test data
        _context.MenuItems.AddRange(
            new MenuItem { Name = "Momos", Description = "Dumplings", Price = 8.95m, Category = MenuCategory.Starters, IsAvailable = true },
            new MenuItem { Name = "Dal Bhat", Description = "Lentil", Price = 14.95m, Category = MenuCategory.Mains, IsAvailable = true },
            new MenuItem { Name = "Kheer", Description = "Pudding", Price = 6.95m, Category = MenuCategory.Desserts, IsAvailable = true },
            new MenuItem { Name = "Hidden Item", Description = "Unavailable", Price = 9.99m, Category = MenuCategory.Mains, IsAvailable = false }
        );
        await _context.SaveChangesAsync();
    }

    public async Task DisposeAsync() => await _context.DisposeAsync();

    [Fact]
    public async Task Handle_NullCategory_ReturnsAllAvailableItems()
    {
        var query = new GetMenuItemsQuery(Category: null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(3, "Only available items should be returned, regardless of category");
    }

    [Fact]
    public async Task Handle_EmptyStringCategory_ReturnsAllAvailableItems()
    {
        var query = new GetMenuItemsQuery(Category: "");

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(3, "Empty category string should return all available items");
    }

    [Fact]
    public async Task Handle_WhitespaceCategory_ReturnsAllAvailableItems()
    {
        var query = new GetMenuItemsQuery(Category: "   ");

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_ValidCategory_ReturnsOnlyMatchingItems()
    {
        var query = new GetMenuItemsQuery(Category: "Mains");

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1, "Only the available Mains item should be returned");
        result[0].Name.Should().Be("Dal Bhat");
    }

    [Fact]
    public async Task Handle_CaseInsensitiveCategory_ReturnsMatchingItems()
    {
        var query = new GetMenuItemsQuery(Category: "starters");

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Momos");
    }

    [Fact]
    public async Task Handle_InvalidCategoryString_ReturnsAllAvailableItems()
    {
        var query = new GetMenuItemsQuery(Category: "NotARealCategory");

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(3, "Invalid category should be ignored, returning all available items");
    }

    [Fact]
    public async Task Handle_UnavailableItems_AreNeverReturned()
    {
        var query = new GetMenuItemsQuery(Category: null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotContain(m => m.Name == "Hidden Item",
            "Items with IsAvailable=false must not be returned");
    }

    [Fact]
    public async Task Handle_ResultItems_HaveCorrectDtoFields()
    {
        var query = new GetMenuItemsQuery(Category: "Desserts");

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
        var dto = result[0];
        dto.Name.Should().Be("Kheer");
        dto.Price.Should().Be(6.95m);
        dto.Category.Should().Be("Desserts");
        dto.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_EmptyDatabase_ReturnsEmptyList()
    {
        // Clear all menu items
        _context.MenuItems.RemoveRange(_context.MenuItems);
        await _context.SaveChangesAsync();

        var query = new GetMenuItemsQuery(Category: null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }
}
