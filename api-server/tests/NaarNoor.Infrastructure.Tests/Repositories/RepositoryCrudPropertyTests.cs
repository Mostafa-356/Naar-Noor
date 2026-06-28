using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using Microsoft.EntityFrameworkCore;
using NaarNoor.Domain.Entities;
using NaarNoor.Domain.Enums;
using NaarNoor.Infrastructure.Tests.Fixtures;
using Xunit;

namespace NaarNoor.Infrastructure.Tests.Repositories;

/// <summary>
/// Property-based tests for CRUD operations on repository entities.
/// These tests validate Create, Read, Update, Delete operations work correctly
/// and that entity state is properly maintained through the repository layer.
///
/// **Validates: Requirement 3.1**
/// **Property 7: CRUD Round-Trip Operations**
/// </summary>
public class RepositoryCrudPropertyTests : RepositoryTestBase
{
    /// <summary>
    /// Property: Create operation - new entities can be created and persisted with correct properties.
    /// For any valid entity, when created and saved to the repository, the entity SHALL be persisted
    /// with all properties intact and retrievable with the same values.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property CreateOperation_WithValidEntity_PersistsAllProperties()
    {
        return Prop.ForAll(
            GenerateValidOrder(),
            order =>
            {
                var task = RunAsync(async () =>
                {
                    // Act - Create
                    DbContext.Orders.Add(order);
                    await DbContext.SaveChangesAsync();
                    var createdId = order.Id;

                    // Assert - Retrieve and verify
                    var retrieved = await DbContext.Orders.FirstOrDefaultAsync(x => x.Id == createdId);
                    
                    return retrieved != null &&
                           retrieved.CustomerName == order.CustomerName &&
                           retrieved.Email == order.Email &&
                           retrieved.TotalAmount == order.TotalAmount;
                });

                return task;
            }
        );
    }

    /// <summary>
    /// Property: Read operation - entities can be retrieved from repository with correct data.
    /// For any created Order entity, when retrieved by ID, the returned entity SHALL have
    /// all properties matching the stored values.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property ReadOperation_RetrievesEntityWithCorrectData()
    {
        return Prop.ForAll(
            GenerateValidOrder(),
            order =>
            {
                var task = RunAsync(async () =>
                {
                    // Arrange - Create
                    DbContext.Orders.Add(order);
                    await DbContext.SaveChangesAsync();
                    var createdId = order.Id;
                    
                    DbContext.Orders.Local.Clear(); // Clear local cache to force DB read

                    // Act - Read
                    var retrieved = await DbContext.Orders.AsNoTracking().FirstOrDefaultAsync(x => x.Id == createdId);

                    // Assert
                    return retrieved != null &&
                           retrieved.Id == createdId &&
                           retrieved.CustomerName == order.CustomerName &&
                           retrieved.Email == order.Email &&
                           retrieved.PhoneNumber == order.PhoneNumber &&
                           retrieved.Type == order.Type &&
                           retrieved.Status == order.Status &&
                           retrieved.TotalAmount == order.TotalAmount;
                });

                return task;
            }
        );
    }

    /// <summary>
    /// Property: Update operation - entity changes are persisted correctly.
    /// For any Order entity, when properties are modified and saved, the changes SHALL be
    /// persisted to the repository and retrievable with updated values.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property UpdateOperation_WithModifiedProperties_PersistsChanges()
    {
        return Prop.ForAll(
            GenerateValidOrder(),
            originalOrder =>
            {
                var task = RunAsync(async () =>
                {
                    // Arrange - Create
                    DbContext.Orders.Add(originalOrder);
                    await DbContext.SaveChangesAsync();
                    var orderId = originalOrder.Id;

                    // Act - Update
                    var tracked = await DbContext.Orders.FirstAsync(x => x.Id == orderId);
                    tracked.CustomerName = "Updated Customer";
                    tracked.TotalAmount = 999m;
                    tracked.Status = OrderStatus.Completed;
                    await DbContext.SaveChangesAsync();

                    DbContext.Orders.Local.Clear();

                    // Assert - Retrieve and verify
                    var updated = await DbContext.Orders.AsNoTracking().FirstAsync(x => x.Id == orderId);
                    
                    return updated.CustomerName == "Updated Customer" &&
                           updated.TotalAmount == 999m &&
                           updated.Status == OrderStatus.Completed;
                });

                return task;
            }
        );
    }

    /// <summary>
    /// Property: Delete operation - entities can be removed from repository.
    /// For any Order entity in the repository, when deleted and saved, the entity SHALL no longer
    /// be retrievable from the repository.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property DeleteOperation_RemovesEntity()
    {
        return Prop.ForAll(
            GenerateValidOrder(),
            order =>
            {
                var task = RunAsync(async () =>
                {
                    // Arrange - Create
                    DbContext.Orders.Add(order);
                    await DbContext.SaveChangesAsync();
                    var orderId = order.Id;

                    // Act - Delete
                    var tracked = await DbContext.Orders.FirstAsync(x => x.Id == orderId);
                    DbContext.Orders.Remove(tracked);
                    await DbContext.SaveChangesAsync();

                    DbContext.Orders.Local.Clear();

                    // Assert - Verify deletion
                    var deleted = await DbContext.Orders.FirstOrDefaultAsync(x => x.Id == orderId);
                    
                    return deleted == null;
                });

                return task;
            }
        );
    }

    /// <summary>
    /// Property: Round-trip operation - Create, Read, Update, Delete cycle maintains consistency.
    /// For any Order entity, executing a complete CRUD cycle (Create → Read → Update → Delete)
    /// SHALL result in consistent state throughout and complete removal at the end.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property RoundTripCrud_CompleteLifecycle_MaintainsConsistency()
    {
        return Prop.ForAll(
            GenerateValidOrder(),
            order =>
            {
                var task = RunAsync(async () =>
                {
                    // Create
                    DbContext.Orders.Add(order);
                    await DbContext.SaveChangesAsync();
                    var orderId = order.Id;

                    // Read
                    DbContext.Orders.Local.Clear();
                    var created = await DbContext.Orders.FirstAsync(x => x.Id == orderId);
                    if (created == null) return false;

                    // Update
                    created.CustomerName = "Modified";
                    await DbContext.SaveChangesAsync();

                    // Verify update
                    DbContext.Orders.Local.Clear();
                    var updated = await DbContext.Orders.FirstAsync(x => x.Id == orderId);
                    if (updated?.CustomerName != "Modified") return false;

                    // Delete
                    DbContext.Orders.Remove(updated);
                    await DbContext.SaveChangesAsync();

                    // Verify deletion
                    DbContext.Orders.Local.Clear();
                    var deleted = await DbContext.Orders.FirstOrDefaultAsync(x => x.Id == orderId);
                    
                    return deleted == null;
                });

                return task;
            }
        );
    }

    /// <summary>
    /// Property: Bulk create operations - multiple entities can be created in sequence.
    /// For any collection of Order entities, when all are created and saved to the repository,
    /// all entities SHALL be persistable and retrievable with correct values.
    /// </summary>
    [Fact]
    public async Task BulkCreateOperation_MultipleEntities_AllPersisted()
    {
        // Arrange - Generate 5 valid orders
        var orders = new List<Order>
        {
            CreateValidOrder("Customer 1", "c1@example.com", 100m),
            CreateValidOrder("Customer 2", "c2@example.com", 200m),
            CreateValidOrder("Customer 3", "c3@example.com", 300m),
            CreateValidOrder("Customer 4", "c4@example.com", 400m),
            CreateValidOrder("Customer 5", "c5@example.com", 500m)
        };

        var orderIds = new List<Guid>();

        // Act - Create all
        foreach (var order in orders)
        {
            DbContext.Orders.Add(order);
            await DbContext.SaveChangesAsync();
            orderIds.Add(order.Id);
        }

        DbContext.Orders.Local.Clear();

        // Assert - Verify all persisted
        var retrieved = await DbContext.Orders
            .Where(x => orderIds.Contains(x.Id))
            .ToListAsync();

        retrieved.Should().HaveCount(5, "All 5 orders should be persisted");
        
        for (int i = 0; i < retrieved.Count; i++)
        {
            retrieved[i].CustomerName.Should().Be($"Customer {i + 1}");
            retrieved[i].TotalAmount.Should().Be((i + 1) * 100m);
        }
    }

    /// <summary>
    /// Property: Update with validation - invalid updates are rejected.
    /// For any Order entity, when updated with invalid data, the operation SHALL fail or be rolled back.
    /// </summary>
    [Fact]
    public async Task UpdateWithInvalidData_ShouldNotPersist()
    {
        // Arrange - Create valid order
        var order = CreateValidOrder("Customer", "customer@example.com", 100m);
        DbContext.Orders.Add(order);
        await DbContext.SaveChangesAsync();
        var orderId = order.Id;

        // Act - Attempt invalid update (empty email)
        var tracked = await DbContext.Orders.FirstAsync(x => x.Id == orderId);
        var originalEmail = tracked.Email;
        tracked.Email = string.Empty; // Invalid

        // Try to save
        try
        {
            await DbContext.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Expected - validation should catch this
        }

        DbContext.Orders.Local.Clear();

        // Assert - Original value should be intact
        var verified = await DbContext.Orders.FirstAsync(x => x.Id == orderId);
        verified.Email.Should().Be(originalEmail, "Invalid update should not persist");
    }

    // ==================== HELPER METHODS ====================

    private Arbitrary<Order> GenerateValidOrder()
    {
        var genOrder = from name in Gen.AlphaNumericString.Where(s => s.Length > 0 && s.Length <= 100)
                       from email in Gen.AlphaNumericString.Where(s => s.Length > 0 && s.Length <= 100)
                       from phone in Gen.AlphaNumericString.Where(s => s.Length > 0 && s.Length <= 20)
                       from amount in Gen.Choose(1, 1000)
                       select CreateValidOrder(
                           name.Length > 0 ? name : "Customer",
                           email.Length > 0 ? email : "customer@example.com",
                           (decimal)amount
                       );

        return Arb.From(genOrder);
    }

    private Order CreateValidOrder(string customerName, string email, decimal amount)
    {
        return new Order
        {
            CustomerName = customerName,
            Email = email,
            PhoneNumber = "555-1234",
            Type = OrderType.DineIn,
            Status = OrderStatus.Pending,
            TotalAmount = amount,
            Items = new List<OrderItem>()
        };
    }

    private bool RunAsync(Func<Task<bool>> func)
    {
        try
        {
            return func().Result;
        }
        catch
        {
            return false;
        }
    }
}
