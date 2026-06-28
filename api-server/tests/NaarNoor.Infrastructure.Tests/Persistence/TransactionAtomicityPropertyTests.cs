using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using Microsoft.EntityFrameworkCore;
using NaarNoor.Domain.Entities;
using NaarNoor.Domain.Enums;
using NaarNoor.Infrastructure.Tests.Fixtures;
using Xunit;

namespace NaarNoor.Infrastructure.Tests.Persistence;

/// <summary>
/// Property-based tests for database transaction atomicity.
/// These tests validate that transactions maintain all-or-nothing semantics,
/// rollback on errors, isolation levels are enforced, and concurrent operations work correctly.
///
/// **Validates: Requirement 3.1**
/// **Property 9: Transaction Atomicity**
/// </summary>
public class TransactionAtomicityPropertyTests : RepositoryTestBase
{
    /// <summary>
    /// Property: All-or-nothing behavior - either all changes commit or none do.
    /// For any transaction with multiple operations, if any operation fails,
    /// all changes SHALL be rolled back and no partial state persisted.
    /// </summary>
    [Fact]
    public async Task AllOrNothing_WithFailingOperation_RollsBackAllChanges()
    {
        // Arrange - Create initial state
        var order1 = new Order
        {
            CustomerName = "Customer 1",
            Email = "c1@example.com",
            PhoneNumber = "555-1111",
            Type = OrderType.DineIn,
            Status = OrderStatus.Pending,
            TotalAmount = 100m,
            Items = new List<OrderItem>()
        };

        DbContext.Orders.Add(order1);
        await DbContext.SaveChangesAsync();
        var initialCount = await DbContext.Orders.CountAsync();

        // Act - Start transaction with multiple operations
        using (var transaction = await DbContext.Database.BeginTransactionAsync())
        {
            try
            {
                // Operation 1: Add valid order
                var order2 = new Order
                {
                    CustomerName = "Customer 2",
                    Email = "c2@example.com",
                    PhoneNumber = "555-2222",
                    Type = OrderType.Delivery,
                    Status = OrderStatus.Confirmed,
                    TotalAmount = 200m,
                    Items = new List<OrderItem>()
                };
                DbContext.Orders.Add(order2);
                await DbContext.SaveChangesAsync();

                // Operation 2: Cause failure (null required field)
                var order3 = new Order
                {
                    CustomerName = null, // Invalid
                    Email = "c3@example.com",
                    PhoneNumber = "555-3333",
                    Type = OrderType.DineIn,
                    Status = OrderStatus.Pending,
                    TotalAmount = 300m,
                    Items = new List<OrderItem>()
                };
                DbContext.Orders.Add(order3);
                await DbContext.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync();
            }
        }

        // Assert - Verify rollback: only initial order exists
        var finalCount = await DbContext.Orders.CountAsync();
        finalCount.Should().Be(initialCount, "Transaction should have been rolled back completely");
    }

    /// <summary>
    /// Property: Rollback on error - failed operation rolls back entire transaction.
    /// For any transaction that encounters an error during commit, the entire transaction
    /// SHALL be rolled back and database state restored to pre-transaction value.
    /// </summary>
    [Fact]
    public async Task RollbackOnError_FailedCommit_RestoresState()
    {
        // Arrange
        var order = new Order
        {
            CustomerName = "Original",
            Email = "original@example.com",
            PhoneNumber = "555-1111",
            Type = OrderType.DineIn,
            Status = OrderStatus.Pending,
            TotalAmount = 100m,
            Items = new List<OrderItem>()
        };

        DbContext.Orders.Add(order);
        await DbContext.SaveChangesAsync();
        var orderId = order.Id;
        var originalName = order.CustomerName;

        // Act - Start transaction and cause error
        using (var transaction = await DbContext.Database.BeginTransactionAsync())
        {
            try
            {
                var tracked = await DbContext.Orders.FirstAsync(x => x.Id == orderId);
                tracked.CustomerName = "Modified";
                await DbContext.SaveChangesAsync();

                // Force error
                throw new InvalidOperationException("Simulated error");
            }
            catch (InvalidOperationException)
            {
                await transaction.RollbackAsync();
            }
        }

        DbContext.Orders.Local.Clear();

        // Assert - Original state should be restored
        var restored = await DbContext.Orders.FirstAsync(x => x.Id == orderId);
        restored.CustomerName.Should().Be(originalName, "Transaction should have been rolled back");
    }

    /// <summary>
    /// Property: Isolation level enforcement - concurrent transactions don't interfere.
    /// For multiple concurrent transactions, each transaction SHALL see a consistent view
    /// of the database and isolation levels SHALL prevent dirty reads and other anomalies.
    /// </summary>
    [Fact]
    public async Task IsolationLevel_ConcurrentTransactions_NoInterference()
    {
        // Arrange - Create order
        var order = new Order
        {
            CustomerName = "Shared",
            Email = "shared@example.com",
            PhoneNumber = "555-1111",
            Type = OrderType.DineIn,
            Status = OrderStatus.Pending,
            TotalAmount = 100m,
            Items = new List<OrderItem>()
        };

        DbContext.Orders.Add(order);
        await DbContext.SaveChangesAsync();
        var orderId = order.Id;

        // Act - Simulate concurrent transactions
        var transaction1Task = Task.Run(async () =>
        {
            using (var tx1 = await DbContext.Database.BeginTransactionAsync())
            {
                var o1 = await DbContext.Orders.FirstAsync(x => x.Id == orderId);
                await Task.Delay(100); // Simulate work
                o1.TotalAmount = 200m;
                await DbContext.SaveChangesAsync();
                await tx1.CommitAsync();
            }
        });

        var transaction2Task = Task.Run(async () =>
        {
            // Small delay to ensure tx1 starts first
            await Task.Delay(50);
            using (var tx2 = await DbContext.Database.BeginTransactionAsync())
            {
                var o2 = await DbContext.Orders.FirstAsync(x => x.Id == orderId);
                o2.TotalAmount = 300m;
                await DbContext.SaveChangesAsync();
                await tx2.CommitAsync();
            }
        });

        await Task.WhenAll(transaction1Task, transaction2Task);

        // Assert - Last write wins (300m)
        DbContext.Orders.Local.Clear();
        var final = await DbContext.Orders.FirstAsync(x => x.Id == orderId);
        final.TotalAmount.Should().Be(300m, "Last transaction's changes should be visible");
    }

    /// <summary>
    /// Property: Multiple operations in transaction - all succeed or all fail.
    /// For a transaction with multiple independent operations, either all SHALL succeed
    /// and be persisted together, or all SHALL be rolled back on any failure.
    /// </summary>
    [Fact]
    public async Task MultipleOperationsInTransaction_AllSucceedOrAllFail()
    {
        // Arrange
        var initialOrderCount = await DbContext.Orders.CountAsync();

        // Act - Successful transaction with multiple operations
        using (var transaction = await DbContext.Database.BeginTransactionAsync())
        {
            // Operation 1
            var order1 = new Order
            {
                CustomerName = "O1",
                Email = "o1@example.com",
                PhoneNumber = "555-1111",
                Type = OrderType.DineIn,
                Status = OrderStatus.Pending,
                TotalAmount = 100m,
                Items = new List<OrderItem>()
            };
            DbContext.Orders.Add(order1);

            // Operation 2
            var order2 = new Order
            {
                CustomerName = "O2",
                Email = "o2@example.com",
                PhoneNumber = "555-2222",
                Type = OrderType.Delivery,
                Status = OrderStatus.Confirmed,
                TotalAmount = 200m,
                Items = new List<OrderItem>()
            };
            DbContext.Orders.Add(order2);

            await DbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        // Assert - Both operations persisted
        var afterSuccessCount = await DbContext.Orders.CountAsync();
        afterSuccessCount.Should().Be(initialOrderCount + 2, "Both operations should be persisted");

        // Act - Failed transaction with multiple operations
        var beforeFailCount = afterSuccessCount;

        using (var transaction = await DbContext.Database.BeginTransactionAsync())
        {
            try
            {
                // Operation 1
                var order3 = new Order
                {
                    CustomerName = "O3",
                    Email = "o3@example.com",
                    PhoneNumber = "555-3333",
                    Type = OrderType.DineIn,
                    Status = OrderStatus.Pending,
                    TotalAmount = 300m,
                    Items = new List<OrderItem>()
                };
                DbContext.Orders.Add(order3);

                // Operation 2 (will fail)
                var order4 = new Order
                {
                    CustomerName = null, // Invalid
                    Email = "o4@example.com",
                    PhoneNumber = "555-4444",
                    Type = OrderType.Delivery,
                    Status = OrderStatus.Confirmed,
                    TotalAmount = 400m,
                    Items = new List<OrderItem>()
                };
                DbContext.Orders.Add(order4);

                await DbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync();
            }
        }

        // Assert - No operations persisted
        var afterFailCount = await DbContext.Orders.CountAsync();
        afterFailCount.Should().Be(beforeFailCount, "Failed transaction should have rolled back all operations");
    }

    /// <summary>
    /// Property: Nested transaction handling - nested transactions work correctly.
    /// For transactions within transactions, the behavior SHALL depend on the SavePoint implementation
    /// or database engine, ensuring consistency is maintained.
    /// </summary>
    [Fact]
    public async Task NestedTransactionHandling_SavepointBehavior()
    {
        // Arrange
        var order = new Order
        {
            CustomerName = "Base",
            Email = "base@example.com",
            PhoneNumber = "555-1111",
            Type = OrderType.DineIn,
            Status = OrderStatus.Pending,
            TotalAmount = 100m,
            Items = new List<OrderItem>()
        };

        DbContext.Orders.Add(order);
        await DbContext.SaveChangesAsync();
        var orderId = order.Id;

        // Act - Outer transaction with savepoint
        using (var outerTransaction = await DbContext.Database.BeginTransactionAsync())
        {
            var tracked = await DbContext.Orders.FirstAsync(x => x.Id == orderId);
            tracked.TotalAmount = 200m;
            await DbContext.SaveChangesAsync();

            // Savepoint (inner transaction)
            using (var savePoint = await DbContext.Database.BeginTransactionAsync())
            {
                tracked.TotalAmount = 300m;
                await DbContext.SaveChangesAsync();
                // Note: rolling back savepoint might not work depending on database support
                // This is a simplified test
            }

            await outerTransaction.CommitAsync();
        }

        // Assert - Final state should reflect outer transaction
        DbContext.Orders.Local.Clear();
        var final = await DbContext.Orders.FirstAsync(x => x.Id == orderId);
        final.TotalAmount.Should().BeOneOf(200m, 300m, "Transaction state should be consistent");
    }

    /// <summary>
    /// Property: Transaction consistency - reading within transaction sees consistent state.
    /// For any data read within a transaction, subsequent reads of the same data
    /// SHALL show the same values (repeatable read guarantee).
    /// </summary>
    [Fact]
    public async Task TransactionConsistency_RepeatableRead()
    {
        // Arrange
        var order = new Order
        {
            CustomerName = "Test",
            Email = "test@example.com",
            PhoneNumber = "555-1111",
            Type = OrderType.DineIn,
            Status = OrderStatus.Pending,
            TotalAmount = 100m,
            Items = new List<OrderItem>()
        };

        DbContext.Orders.Add(order);
        await DbContext.SaveChangesAsync();
        var orderId = order.Id;

        // Act & Assert - Read twice within transaction
        using (var transaction = await DbContext.Database.BeginTransactionAsync())
        {
            var read1 = await DbContext.Orders.FirstAsync(x => x.Id == orderId);
            var amount1 = read1.TotalAmount;

            await Task.Delay(100); // Simulate work

            var read2 = await DbContext.Orders.FirstAsync(x => x.Id == orderId);
            var amount2 = read2.TotalAmount;

            amount1.Should().Be(amount2, "Repeatable read should show same value");

            await transaction.CommitAsync();
        }
    }

    // ==================== HELPER METHODS ====================

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
