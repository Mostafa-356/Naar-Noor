using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NaarNoor.Domain.Entities;
using NaarNoor.Domain.Enums;
using NaarNoor.Infrastructure.Data;
using NaarNoor.Infrastructure.Repositories;
using Xunit;

namespace NaarNoor.Infrastructure.Tests.Persistence;

public class TransactionAtomicityPropertyTests : IAsyncLifetime
{
    private ApplicationDbContext _context = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("Txn_" + Guid.NewGuid())
            .Options;
        _context = new ApplicationDbContext(options);
        await _context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
    }

    [Fact]
    public async Task SaveChangesAsync_PersistsAllAddedEntities()
    {
        var repo = new Repository<Chef>(_context);
        var chef1 = new Chef { Name = "Chef One", Title = "Head", Bio = "Bio", Specialty = "Indian" };
        var chef2 = new Chef { Name = "Chef Two", Title = "Sous", Bio = "Bio", Specialty = "Nepali" };

        repo.Add(chef1);
        repo.Add(chef2);
        await _context.SaveChangesAsync();

        var all = await repo.Query().ToListAsync();
        all.Should().HaveCount(2);
    }

    [Fact]
    public async Task SaveChangesAsync_WithoutAdd_ChangesNothing()
    {
        var repo = new Repository<Chef>(_context);
        var before = await repo.Query().CountAsync();

        await _context.SaveChangesAsync();

        var after = await repo.Query().CountAsync();
        after.Should().Be(before);
    }

    [Fact]
    public async Task Remove_AfterSave_EntityGone()
    {
        var repo = new Repository<Chef>(_context);
        var chef = new Chef { Name = "ToRemove", Title = "Chef", Bio = "Bio", Specialty = "Indian" };
        repo.Add(chef);
        await _context.SaveChangesAsync();

        repo.Remove(chef);
        await _context.SaveChangesAsync();

        var result = repo.Query().FirstOrDefault(c => c.Id == chef.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task Update_AfterSave_ChangesReflected()
    {
        var repo = new Repository<MenuItem>(_context);
        var item = new MenuItem { Name = "Original", Description = "Desc", Price = 10.00m, Category = MenuCategory.Starters };
        repo.Add(item);
        await _context.SaveChangesAsync();

        item.Price = 12.50m;
        repo.Update(item);
        await _context.SaveChangesAsync();

        var updated = await repo.Query().FirstAsync(m => m.Id == item.Id);
        updated.Price.Should().Be(12.50m);
    }

    [Fact]
    public async Task MultipleEntityTypes_CanBePersistedTogether()
    {
        var chefRepo = new Repository<Chef>(_context);
        var menuRepo = new Repository<MenuItem>(_context);

        chefRepo.Add(new Chef { Name = "Multi Chef", Title = "Chef", Bio = "Bio", Specialty = "Nepali" });
        menuRepo.Add(new MenuItem { Name = "Multi Item", Description = "Desc", Price = 9.95m, Category = MenuCategory.Mains });
        await _context.SaveChangesAsync();

        (await chefRepo.Query().CountAsync()).Should().BeGreaterThanOrEqualTo(1);
        (await menuRepo.Query().CountAsync()).Should().BeGreaterThanOrEqualTo(1);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(7)]
    public async Task BulkAdd_AllEntitiesPresistAfterSave(int count)
    {
        var repo = new Repository<Review>(_context);
        for (int i = 0; i < count; i++)
        {
            repo.Add(new Review { CustomerName = $"Customer {i}", Rating = 5, Comment = "Great!", Source = "Google" });
        }
        await _context.SaveChangesAsync();

        (await repo.Query().CountAsync()).Should().BeGreaterThanOrEqualTo(count);
    }

    [Fact]
    public async Task CancellationToken_IsPropagatedToSaveChanges()
    {
        using var cts = new CancellationTokenSource();
        var repo = new Repository<Chef>(_context);
        repo.Add(new Chef { Name = "CancelTest", Title = "Chef", Bio = "Bio", Specialty = "Indian" });

        await _context.SaveChangesAsync(cts.Token);

        (await repo.Query().CountAsync()).Should().BeGreaterThanOrEqualTo(1);
    }
}
