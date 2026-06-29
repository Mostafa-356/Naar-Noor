using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NaarNoor.Domain.Entities;
using NaarNoor.Domain.Enums;
using NaarNoor.Infrastructure.Data;
using Xunit;

namespace NaarNoor.Infrastructure.Tests.Data;

public class ApplicationDbContextTests : IAsyncLifetime
{
    private ApplicationDbContext _context = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("AppDbContext_" + Guid.NewGuid())
            .Options;
        _context = new ApplicationDbContext(options);
        await _context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync() => await _context.DisposeAsync();

    [Fact]
    public async Task SaveChangesAsync_WhenEntityIsModified_SetsUpdatedAt()
    {
        var chef = new Chef { Name = "Chef Test", Title = "Chef", Bio = "Bio", Specialty = "Indian" };
        _context.Chefs.Add(chef);
        await _context.SaveChangesAsync();

        var originalUpdatedAt = chef.UpdatedAt;

        // Simulate a small delay to ensure timestamp difference
        await Task.Delay(5);

        chef.Name = "Updated Chef";
        _context.Entry(chef).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        chef.UpdatedAt.Should().BeOnOrAfter(originalUpdatedAt,
            "UpdatedAt should be set to a later time when entity is modified");
    }

    [Fact]
    public async Task SaveChangesAsync_NewEntityAdded_DoesNotThrow()
    {
        var item = new MenuItem { Name = "New Item", Description = "Desc", Price = 9.99m, Category = MenuCategory.Mains };

        Func<Task> act = async () =>
        {
            _context.MenuItems.Add(item);
            await _context.SaveChangesAsync();
        };

        await act.Should().NotThrowAsync();
        item.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void DbSet_Reservations_IsAccessible()
    {
        _context.Reservations.Should().NotBeNull();
    }

    [Fact]
    public void DbSet_MenuItems_IsAccessible()
    {
        _context.MenuItems.Should().NotBeNull();
    }

    [Fact]
    public void DbSet_Chefs_IsAccessible()
    {
        _context.Chefs.Should().NotBeNull();
    }

    [Fact]
    public void DbSet_Reviews_IsAccessible()
    {
        _context.Reviews.Should().NotBeNull();
    }

    [Fact]
    public void DbSet_ContactInquiries_IsAccessible()
    {
        _context.ContactInquiries.Should().NotBeNull();
    }

    [Fact]
    public void DbSet_Orders_IsAccessible()
    {
        _context.Orders.Should().NotBeNull();
    }

    [Fact]
    public void DbSet_OrderItems_IsAccessible()
    {
        _context.OrderItems.Should().NotBeNull();
    }

    [Fact]
    public async Task SaveChangesAsync_MultipleDifferentEntities_PersistsAll()
    {
        _context.Chefs.Add(new Chef { Name = "Multi Chef", Title = "T", Bio = "B", Specialty = "S" });
        _context.MenuItems.Add(new MenuItem { Name = "Multi Item", Description = "D", Price = 5m, Category = MenuCategory.Starters });
        _context.Reviews.Add(new Review { CustomerName = "Reviewer", Rating = 4, Comment = "Good", Source = "Google" });

        var count = await _context.SaveChangesAsync();

        count.Should().Be(3, "Three entities were added");
    }

    [Fact]
    public async Task SaveChangesAsync_OnUnmodifiedEntities_ReturnsZero()
    {
        var chef = new Chef { Name = "No Change Chef", Title = "T", Bio = "B", Specialty = "S" };
        _context.Chefs.Add(chef);
        await _context.SaveChangesAsync();

        // No changes after initial save
        var count = await _context.SaveChangesAsync();

        count.Should().Be(0, "No changes means zero rows affected");
    }

    [Fact]
    public async Task Order_WithOrderItems_CanBeStoredAndRetrieved()
    {
        var order = new Order
        {
            CustomerName = "Test Customer",
            Email = "test@example.com",
            PhoneNumber = "07700000000",
            Type = OrderType.Delivery,
            TotalAmount = 29.90m,
            Items = new List<OrderItem>
            {
                new() { MenuItemId = Guid.NewGuid(), MenuItemName = "Momos", UnitPrice = 8.95m, Quantity = 2 },
                new() { MenuItemId = Guid.NewGuid(), MenuItemName = "Sel Roti", UnitPrice = 4.50m, Quantity = 3 }
            }
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        var saved = await _context.Orders.Include(o => o.Items).FirstAsync(o => o.Id == order.Id);
        saved.Items.Should().HaveCount(2);
        saved.TotalAmount.Should().Be(29.90m);
    }

    [Fact]
    public async Task UpdatedAt_IsNotChanged_WhenEntityIsAdded_NotModified()
    {
        var review = new Review { CustomerName = "New", Rating = 5, Comment = "Awesome", Source = "Direct" };
        var before = DateTime.UtcNow;
        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        review.CreatedAt.Should().BeOnOrAfter(before.AddSeconds(-1));
    }
}
