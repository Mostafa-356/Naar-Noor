using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NaarNoor.Domain.Entities;
using NaarNoor.Domain.Enums;
using NaarNoor.Infrastructure.Data;
using NaarNoor.Infrastructure.Repositories;
using Xunit;

namespace NaarNoor.Infrastructure.Tests.Data;

public class DatabaseSeederTests : IAsyncLifetime
{
    private ApplicationDbContext _context = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("Seeder_" + Guid.NewGuid())
            .Options;
        _context = new ApplicationDbContext(options);
        await _context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync() => await _context.DisposeAsync();

    [Fact]
    public async Task MenuItems_Repository_CanStoreAndRetrieve()
    {
        var repo = new Repository<MenuItem>(_context);
        repo.Add(new MenuItem { Name = "Dal Bhat", Description = "Lentil rice", Price = 14.95m, Category = MenuCategory.Mains, IsVegetarian = true });
        repo.Add(new MenuItem { Name = "Sel Roti", Description = "Rice donut", Price = 4.50m, Category = MenuCategory.Starters });
        repo.Add(new MenuItem { Name = "Momos", Description = "Dumplings", Price = 8.95m, Category = MenuCategory.Starters });
        await _context.SaveChangesAsync();

        var all = await _context.MenuItems.ToListAsync();
        all.Should().HaveCount(3);
        all.Should().OnlyContain(m => m.Price > 0);
        all.Should().OnlyContain(m => !string.IsNullOrWhiteSpace(m.Name));
    }

    [Fact]
    public async Task Chefs_Repository_CanStoreAndRetrieve()
    {
        var repo = new Repository<Chef>(_context);
        repo.Add(new Chef { Name = "Chef Binod", Title = "Executive Chef", Bio = "Master of Himalayan cuisine.", Specialty = "Nepali", IsActive = true, SortOrder = 1 });
        repo.Add(new Chef { Name = "Chef Priya", Title = "Sous Chef", Bio = "Expert in Indian spices.", Specialty = "Indian", IsActive = true, SortOrder = 2 });
        await _context.SaveChangesAsync();

        var all = await _context.Chefs.ToListAsync();
        all.Should().HaveCount(2);
        all.Should().OnlyContain(c => c.IsActive);
    }

    [Fact]
    public async Task Reviews_Repository_CanStoreAndRetrieve()
    {
        var repo = new Repository<Review>(_context);
        repo.Add(new Review { CustomerName = "John Smith", Rating = 5, Comment = "Excellent!", Source = "Google", IsApproved = true });
        repo.Add(new Review { CustomerName = "Jane Doe", Rating = 4, Comment = "Very good", Source = "TripAdvisor", IsApproved = true });
        await _context.SaveChangesAsync();

        var all = await _context.Reviews.ToListAsync();
        all.Should().HaveCount(2);
        all.Should().OnlyContain(r => r.Rating >= 1 && r.Rating <= 5);
    }

    [Fact]
    public async Task AllEntityTypes_CanBeQueriedTogether()
    {
        _context.MenuItems.Add(new MenuItem { Name = "Item", Description = "D", Price = 5m, Category = MenuCategory.Starters });
        _context.Chefs.Add(new Chef { Name = "Chef", Title = "T", Bio = "B", Specialty = "S" });
        _context.Reviews.Add(new Review { CustomerName = "Customer", Rating = 5, Comment = "Great", Source = "Google" });
        _context.Reservations.Add(new Reservation
        {
            CustomerName = "Guest",
            Email = "g@t.com",
            PhoneNumber = "0770",
            ReservationDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            ReservationTime = new TimeOnly(19, 0),
            PartySize = 2
        });
        _context.ContactInquiries.Add(new ContactInquiry { Name = "User", Email = "u@t.com", Subject = "Hello", Message = "World" });
        await _context.SaveChangesAsync();

        (await _context.MenuItems.CountAsync()).Should().BeGreaterThan(0);
        (await _context.Chefs.CountAsync()).Should().BeGreaterThan(0);
        (await _context.Reviews.CountAsync()).Should().BeGreaterThan(0);
        (await _context.Reservations.CountAsync()).Should().BeGreaterThan(0);
        (await _context.ContactInquiries.CountAsync()).Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task MenuItem_IsAvailable_DefaultsToTrue()
    {
        _context.MenuItems.Add(new MenuItem { Name = "Default Item", Description = "D", Price = 9.99m, Category = MenuCategory.Mains });
        await _context.SaveChangesAsync();

        var item = await _context.MenuItems.FirstAsync();
        item.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task Review_IsApproved_CanBeSet()
    {
        _context.Reviews.Add(new Review { CustomerName = "Test", Rating = 5, Comment = "Great", Source = "Google", IsApproved = true });
        _context.Reviews.Add(new Review { CustomerName = "Other", Rating = 3, Comment = "Ok", Source = "Yelp", IsApproved = false });
        await _context.SaveChangesAsync();

        var approved = await _context.Reviews.Where(r => r.IsApproved).ToListAsync();
        approved.Should().HaveCount(1);
    }

    [Fact]
    public async Task MenuItem_Categories_AreStoredCorrectly()
    {
        _context.MenuItems.Add(new MenuItem { Name = "Starter", Description = "D", Price = 5m, Category = MenuCategory.Starters });
        _context.MenuItems.Add(new MenuItem { Name = "Main", Description = "D", Price = 15m, Category = MenuCategory.Mains });
        _context.MenuItems.Add(new MenuItem { Name = "Dessert", Description = "D", Price = 8m, Category = MenuCategory.Desserts });
        await _context.SaveChangesAsync();

        var starters = await _context.MenuItems.Where(m => m.Category == MenuCategory.Starters).ToListAsync();
        var mains = await _context.MenuItems.Where(m => m.Category == MenuCategory.Mains).ToListAsync();
        starters.Should().HaveCount(1);
        mains.Should().HaveCount(1);
    }
}
