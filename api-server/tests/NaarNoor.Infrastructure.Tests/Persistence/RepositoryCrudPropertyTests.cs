using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NaarNoor.Domain.Entities;
using NaarNoor.Domain.Enums;
using NaarNoor.Infrastructure.Data;
using NaarNoor.Infrastructure.Repositories;
using Xunit;

namespace NaarNoor.Infrastructure.Tests.Persistence;

public class RepositoryCrudPropertyTests : IAsyncLifetime
{
    private ApplicationDbContext _context = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("RepoCrud_" + Guid.NewGuid())
            .Options;
        _context = new ApplicationDbContext(options);
        await _context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
    }

    [Fact]
    public void Add_Chef_AppearsInQuery()
    {
        var repo = new Repository<Chef>(_context);
        var chef = new Chef { Name = "Ahmad", Title = "Head Chef", Bio = "Expert", Specialty = "Himalayan" };

        repo.Add(chef);
        _context.SaveChanges();

        var result = repo.Query().ToList();
        result.Should().ContainSingle(c => c.Name == "Ahmad");
    }

    [Fact]
    public void Add_MultipleChefs_AllAppearInQuery()
    {
        var repo = new Repository<Chef>(_context);
        repo.Add(new Chef { Name = "Chef A", Title = "Senior", Bio = "Bio A", Specialty = "Indian" });
        repo.Add(new Chef { Name = "Chef B", Title = "Junior", Bio = "Bio B", Specialty = "Nepali" });
        _context.SaveChanges();

        repo.Query().Count().Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public void Query_WithFilter_ReturnsMatchingEntities()
    {
        var repo = new Repository<Chef>(_context);
        repo.Add(new Chef { Name = "Active Chef", Title = "Chef", Bio = "Bio", Specialty = "Indian", IsActive = true });
        repo.Add(new Chef { Name = "Inactive Chef", Title = "Chef", Bio = "Bio", Specialty = "Indian", IsActive = false });
        _context.SaveChanges();

        var active = repo.Query().Where(c => c.IsActive).ToList();
        active.Should().OnlyContain(c => c.IsActive);
    }

    [Fact]
    public void Update_Chef_ChangesArePersisted()
    {
        var repo = new Repository<Chef>(_context);
        var chef = new Chef { Name = "Old Name", Title = "Chef", Bio = "Bio", Specialty = "Indian" };
        repo.Add(chef);
        _context.SaveChanges();

        chef.Name = "New Name";
        repo.Update(chef);
        _context.SaveChanges();

        var result = repo.Query().First(c => c.Id == chef.Id);
        result.Name.Should().Be("New Name");
    }

    [Fact]
    public void Remove_Chef_IsRemovedFromQuery()
    {
        var repo = new Repository<Chef>(_context);
        var chef = new Chef { Name = "To Delete", Title = "Chef", Bio = "Bio", Specialty = "Indian" };
        repo.Add(chef);
        _context.SaveChanges();

        repo.Remove(chef);
        _context.SaveChanges();

        var result = repo.Query().FirstOrDefault(c => c.Id == chef.Id);
        result.Should().BeNull();
    }

    [Fact]
    public void Add_MenuItem_AppearsInQuery()
    {
        var repo = new Repository<MenuItem>(_context);
        var item = new MenuItem { Name = "Momos", Description = "Himalayan dumplings", Price = 8.95m, Category = MenuCategory.Starters };
        repo.Add(item);
        _context.SaveChanges();

        var result = repo.Query().First(m => m.Id == item.Id);
        result.Name.Should().Be("Momos");
        result.Price.Should().Be(8.95m);
    }

    [Fact]
    public void Add_Review_AppearsInQuery()
    {
        var repo = new Repository<Review>(_context);
        var review = new Review { CustomerName = "John Doe", Rating = 5, Comment = "Excellent!", Source = "Google" };
        repo.Add(review);
        _context.SaveChanges();

        var result = repo.Query().First(r => r.Id == review.Id);
        result.CustomerName.Should().Be("John Doe");
        result.Rating.Should().Be(5);
    }

    [Fact]
    public void Add_ContactInquiry_AppearsInQuery()
    {
        var repo = new Repository<ContactInquiry>(_context);
        var inquiry = new ContactInquiry { Name = "Jane", Email = "jane@test.com", Subject = "Booking", Message = "I'd like to book" };
        repo.Add(inquiry);
        _context.SaveChanges();

        repo.Query().Should().ContainSingle(c => c.Email == "jane@test.com");
    }

    [Fact]
    public void Add_Reservation_AppearsInQuery()
    {
        var repo = new Repository<Reservation>(_context);
        var res = new Reservation
        {
            CustomerName = "Ali Hassan",
            Email = "ali@test.com",
            PhoneNumber = "07700000001",
            ReservationDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            ReservationTime = new TimeOnly(19, 0),
            PartySize = 4
        };
        repo.Add(res);
        _context.SaveChanges();

        repo.Query().Should().ContainSingle(r => r.Email == "ali@test.com");
    }

    [Fact]
    public void Add_Order_AppearsInQuery()
    {
        var repo = new Repository<Order>(_context);
        var order = new Order
        {
            CustomerName = "Sara Magar",
            Email = "sara@test.com",
            PhoneNumber = "07911123456",
            Type = OrderType.Collection,
            TotalAmount = 25.00m
        };
        repo.Add(order);
        _context.SaveChanges();

        repo.Query().Should().ContainSingle(o => o.Email == "sara@test.com");
    }

    [Fact]
    public void Query_ReturnsIQueryable_SupportingLinqChain()
    {
        var repo = new Repository<MenuItem>(_context);
        repo.Add(new MenuItem { Name = "Dal Bhat", Description = "Lentil rice", Price = 14.95m, Category = MenuCategory.Mains, IsVegetarian = true });
        repo.Add(new MenuItem { Name = "Lamb Rogan Josh", Description = "Slow braised lamb", Price = 18.95m, Category = MenuCategory.Mains });
        _context.SaveChanges();

        var vegItems = repo.Query().Where(m => m.IsVegetarian).OrderBy(m => m.Price).ToList();
        vegItems.Should().OnlyContain(m => m.IsVegetarian);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void Add_MultipleMenuItems_AllPersisted(int count)
    {
        var repo = new Repository<MenuItem>(_context);
        for (int i = 0; i < count; i++)
        {
            repo.Add(new MenuItem { Name = $"Item {i}", Description = "Desc", Price = 9.99m + i, Category = MenuCategory.Starters });
        }
        _context.SaveChanges();

        repo.Query().Count().Should().BeGreaterThanOrEqualTo(count);
    }
}
