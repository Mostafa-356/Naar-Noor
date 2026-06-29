using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NaarNoor.Domain.Entities;
using NaarNoor.Domain.Enums;
using NaarNoor.Infrastructure.Data;
using NaarNoor.Infrastructure.Repositories;
using Xunit;

namespace NaarNoor.Infrastructure.Tests.Persistence;

public class ReferentialIntegrityPropertyTests : IAsyncLifetime
{
    private ApplicationDbContext _context = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("RefInt_" + Guid.NewGuid())
            .Options;
        _context = new ApplicationDbContext(options);
        await _context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
    }

    [Fact]
    public async Task Order_WithItems_ItemsLoadedViaNavigation()
    {
        var orderRepo = new Repository<Order>(_context);
        var order = new Order
        {
            CustomerName = "Test Customer",
            Email = "test@test.com",
            PhoneNumber = "07700000000",
            Type = OrderType.Collection,
            TotalAmount = 20.00m,
            Items = new List<OrderItem>
            {
                new OrderItem { MenuItemName = "Momos", UnitPrice = 8.95m, Quantity = 2 },
                new OrderItem { MenuItemName = "Lassi", UnitPrice = 4.50m, Quantity = 1 }
            }
        };
        orderRepo.Add(order);
        await _context.SaveChangesAsync();

        var saved = await _context.Orders.Include(o => o.Items).FirstAsync(o => o.Id == order.Id);
        saved.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task MenuItem_Properties_AreSavedCorrectly()
    {
        var repo = new Repository<MenuItem>(_context);
        var item = new MenuItem
        {
            Name = "Veg Momos",
            Description = "Handcrafted dumplings",
            Price = 7.95m,
            Category = MenuCategory.Starters,
            IsVegetarian = true,
            IsVegan = true,
            IsGlutenFree = false,
            IsAvailable = true,
            SortOrder = 1
        };
        repo.Add(item);
        await _context.SaveChangesAsync();

        var saved = await repo.Query().FirstAsync(m => m.Id == item.Id);
        saved.IsVegetarian.Should().BeTrue();
        saved.IsVegan.Should().BeTrue();
        saved.Category.Should().Be(MenuCategory.Starters);
        saved.Price.Should().Be(7.95m);
    }

    [Fact]
    public async Task Reservation_Properties_AreSavedCorrectly()
    {
        var repo = new Repository<Reservation>(_context);
        var date = DateOnly.FromDateTime(DateTime.Today.AddDays(7));
        var time = new TimeOnly(19, 30);
        var reservation = new Reservation
        {
            CustomerName = "Priya Sharma",
            Email = "priya@example.com",
            PhoneNumber = "07700900001",
            ReservationDate = date,
            ReservationTime = time,
            PartySize = 4,
            SpecialRequests = "Window seat",
            Status = ReservationStatus.Pending
        };
        repo.Add(reservation);
        await _context.SaveChangesAsync();

        var saved = await repo.Query().FirstAsync(r => r.Id == reservation.Id);
        saved.CustomerName.Should().Be("Priya Sharma");
        saved.ReservationDate.Should().Be(date);
        saved.PartySize.Should().Be(4);
        saved.Status.Should().Be(ReservationStatus.Pending);
    }

    [Fact]
    public async Task ContactInquiry_Properties_AreSavedCorrectly()
    {
        var repo = new Repository<ContactInquiry>(_context);
        var inquiry = new ContactInquiry
        {
            Name = "James Wilson",
            Email = "james@test.com",
            Subject = "Event Booking",
            Message = "I would like to book a private event for 30 people.",
            IsRead = false
        };
        repo.Add(inquiry);
        await _context.SaveChangesAsync();

        var saved = await repo.Query().FirstAsync(c => c.Id == inquiry.Id);
        saved.Subject.Should().Be("Event Booking");
        saved.IsRead.Should().BeFalse();
    }

    [Fact]
    public async Task Chef_Properties_AreSavedCorrectly()
    {
        var repo = new Repository<Chef>(_context);
        var chef = new Chef
        {
            Name = "Chef Binod",
            Title = "Executive Chef",
            Bio = "Master of Himalayan cuisine.",
            Specialty = "Nepali",
            IsActive = true,
            SortOrder = 1
        };
        repo.Add(chef);
        await _context.SaveChangesAsync();

        var saved = await repo.Query().FirstAsync(c => c.Id == chef.Id);
        saved.Title.Should().Be("Executive Chef");
        saved.IsActive.Should().BeTrue();
        saved.SortOrder.Should().Be(1);
    }

    [Fact]
    public async Task Order_StatusDefaults_ArePending()
    {
        var repo = new Repository<Order>(_context);
        var order = new Order
        {
            CustomerName = "Test",
            Email = "test@test.com",
            PhoneNumber = "0770000000",
            Type = OrderType.Collection,
            TotalAmount = 15.00m
        };
        repo.Add(order);
        await _context.SaveChangesAsync();

        var saved = await repo.Query().FirstAsync(o => o.Id == order.Id);
        saved.Status.Should().Be(OrderStatus.Pending);
        saved.PaymentStatus.Should().Be(PaymentStatus.Pending);
    }

    [Fact]
    public async Task BaseEntity_AutoGeneratesId_AndTimestamps()
    {
        var repo = new Repository<Chef>(_context);
        var chef = new Chef { Name = "Auto ID", Title = "Chef", Bio = "Bio", Specialty = "Indian" };
        repo.Add(chef);
        await _context.SaveChangesAsync();

        chef.Id.Should().NotBe(Guid.Empty);
        chef.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
