using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NaarNoor.Domain.Entities;
using NaarNoor.Domain.Enums;
using NaarNoor.Infrastructure.Data;
using NaarNoor.Infrastructure.Repositories;
using Xunit;

namespace NaarNoor.Infrastructure.Tests;

public class UnitOfWorkTests : IAsyncLifetime
{
    private ApplicationDbContext _context = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("UoW_" + Guid.NewGuid())
            .Options;
        _context = new ApplicationDbContext(options);
        await _context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync() => await _context.DisposeAsync();

    [Fact]
    public void Reservations_ReturnsSameInstance_OnMultipleCalls()
    {
        var uow = new UnitOfWork(_context);
        var r1 = uow.Reservations;
        var r2 = uow.Reservations;
        r1.Should().BeSameAs(r2, "UnitOfWork should return cached repository instance");
    }

    [Fact]
    public void MenuItems_ReturnsSameInstance_OnMultipleCalls()
    {
        var uow = new UnitOfWork(_context);
        uow.MenuItems.Should().BeSameAs(uow.MenuItems);
    }

    [Fact]
    public void Chefs_ReturnsSameInstance_OnMultipleCalls()
    {
        var uow = new UnitOfWork(_context);
        uow.Chefs.Should().BeSameAs(uow.Chefs);
    }

    [Fact]
    public void Reviews_ReturnsSameInstance_OnMultipleCalls()
    {
        var uow = new UnitOfWork(_context);
        uow.Reviews.Should().BeSameAs(uow.Reviews);
    }

    [Fact]
    public void ContactInquiries_ReturnsSameInstance_OnMultipleCalls()
    {
        var uow = new UnitOfWork(_context);
        uow.ContactInquiries.Should().BeSameAs(uow.ContactInquiries);
    }

    [Fact]
    public void Orders_ReturnsSameInstance_OnMultipleCalls()
    {
        var uow = new UnitOfWork(_context);
        uow.Orders.Should().BeSameAs(uow.Orders);
    }

    [Fact]
    public void OrderItems_ReturnsSameInstance_OnMultipleCalls()
    {
        var uow = new UnitOfWork(_context);
        uow.OrderItems.Should().BeSameAs(uow.OrderItems);
    }

    [Fact]
    public async Task SaveChangesAsync_PersistsChefAddedViaRepository()
    {
        var uow = new UnitOfWork(_context);
        var chef = new Chef { Name = "UoW Chef", Title = "Chef", Bio = "Bio", Specialty = "Nepali" };

        uow.Chefs.Add(chef);
        var count = await uow.SaveChangesAsync();

        count.Should().Be(1);
        uow.Chefs.Query().Should().ContainSingle(c => c.Name == "UoW Chef");
    }

    [Fact]
    public async Task SaveChangesAsync_PersistsMenuItemAddedViaRepository()
    {
        var uow = new UnitOfWork(_context);
        var item = new MenuItem { Name = "Dal Bhat", Description = "Nepal national dish", Price = 14.95m, Category = MenuCategory.Mains };

        uow.MenuItems.Add(item);
        await uow.SaveChangesAsync();

        uow.MenuItems.Query().Should().ContainSingle(m => m.Name == "Dal Bhat");
    }

    [Fact]
    public async Task SaveChangesAsync_WithCancellationToken_Works()
    {
        var uow = new UnitOfWork(_context);
        var review = new Review { CustomerName = "Tom", Rating = 4, Comment = "Good!", Source = "Yelp" };

        uow.Reviews.Add(review);
        var result = await uow.SaveChangesAsync(CancellationToken.None);

        result.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task AllRepositories_AreAccessible_AndFunctional()
    {
        var uow = new UnitOfWork(_context);

        uow.Chefs.Add(new Chef { Name = "Chef", Title = "T", Bio = "B", Specialty = "S" });
        uow.MenuItems.Add(new MenuItem { Name = "Item", Description = "D", Price = 5.00m, Category = MenuCategory.Starters });
        uow.Reviews.Add(new Review { CustomerName = "Customer", Rating = 5, Comment = "Great", Source = "Google" });
        uow.ContactInquiries.Add(new ContactInquiry { Name = "Inquirer", Email = "i@test.com", Subject = "Test", Message = "Hello" });
        uow.Reservations.Add(new Reservation
        {
            CustomerName = "Guest",
            Email = "guest@test.com",
            PhoneNumber = "0770000000",
            ReservationDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            ReservationTime = new TimeOnly(19, 0),
            PartySize = 2
        });
        uow.Orders.Add(new Order
        {
            CustomerName = "Customer",
            Email = "c@test.com",
            PhoneNumber = "0770000000",
            Type = OrderType.Collection,
            TotalAmount = 10.00m
        });

        var saved = await uow.SaveChangesAsync();
        saved.Should().Be(6);
    }

    [Fact]
    public async Task Remove_ViaUnitOfWork_RemovesEntity()
    {
        var uow = new UnitOfWork(_context);
        var chef = new Chef { Name = "To Remove", Title = "Chef", Bio = "Bio", Specialty = "Indian" };
        uow.Chefs.Add(chef);
        await uow.SaveChangesAsync();

        uow.Chefs.Remove(chef);
        await uow.SaveChangesAsync();

        uow.Chefs.Query().FirstOrDefault(c => c.Id == chef.Id).Should().BeNull();
    }

    [Fact]
    public async Task Update_ViaUnitOfWork_UpdatesEntity()
    {
        var uow = new UnitOfWork(_context);
        var item = new MenuItem { Name = "Old Price Item", Description = "Desc", Price = 9.00m, Category = MenuCategory.Mains };
        uow.MenuItems.Add(item);
        await uow.SaveChangesAsync();

        item.Price = 11.50m;
        uow.MenuItems.Update(item);
        await uow.SaveChangesAsync();

        var updated = uow.MenuItems.Query().First(m => m.Id == item.Id);
        updated.Price.Should().Be(11.50m);
    }
}
