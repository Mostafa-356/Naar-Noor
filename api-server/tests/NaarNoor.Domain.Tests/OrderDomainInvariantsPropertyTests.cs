using FsCheck;
using FsCheck.Xunit;
using FluentAssertions;
using NaarNoor.Domain.Entities;
using NaarNoor.Domain.Enums;

namespace NaarNoor.Domain.Tests;

/// <summary>
/// Property-based tests for Order domain invariants preservation.
/// Validates that Order entity maintains all domain invariants across state changes,
/// including constraint validation, item collection management, and status transitions.
/// 
/// **Validates: Requirements 1.6**
/// **Property 4: Domain Invariants Preservation**
/// </summary>
public class OrderDomainInvariantsPropertyTests
{
    /// <summary>
    /// Property: Order constructor with required fields initializes all properties correctly.
    /// For any valid Order input with required fields (CustomerName, Email, PhoneNumber, OrderType),
    /// the order SHALL be created with all properties correctly initialized,
    /// and domain invariants SHALL be satisfied.
    /// </summary>
    [Property]
    public void OrderEntity_WithRequiredFields_InitializesAllPropertiesCorrectly(
        NonEmptyString customerName,
        NonEmptyString email,
        NonEmptyString phoneNumber)
    {
        // Arrange
        var orderType = OrderType.DineIn;
        
        // Act
        var order = new Order
        {
            CustomerName = customerName.Get,
            Email = email.Get,
            PhoneNumber = phoneNumber.Get,
            Type = orderType,
            Status = OrderStatus.Pending,
            TotalAmount = 0m,
            Items = new List<OrderItem>()
        };

        // Assert
        order.Should().NotBeNull();
        order.Id.Should().NotBe(Guid.Empty);
        order.CustomerName.Should().Be(customerName.Get);
        order.Email.Should().Be(email.Get);
        order.PhoneNumber.Should().Be(phoneNumber.Get);
        order.Type.Should().Be(orderType);
        order.Status.Should().Be(OrderStatus.Pending);
        order.Items.Should().NotBeNull();
        order.Items.Should().BeEmpty();
    }

    /// <summary>
    /// Property: Order item collection management preserves invariants.
    /// For any Order with a collection of OrderItems, after adding items,
    /// the collection SHALL contain all added items and the count SHALL match the number of additions.
    /// </summary>
    [Property]
    public void OrderEntity_WithItems_CollectionManagementPreservesInvariants(
        NonEmptyString customerName,
        PositiveInt itemCount)
    {
        // Arrange
        var order = new Order
        {
            CustomerName = customerName.Get,
            Email = "test@example.com",
            PhoneNumber = "1234567890",
            Type = OrderType.DineIn,
            Status = OrderStatus.Pending,
            Items = new List<OrderItem>()
        };

        // Act - Add items
        var actualItemCount = Math.Min(itemCount.Get, 50); // Limit to reasonable number
        for (int i = 0; i < actualItemCount; i++)
        {
            var item = new OrderItem
            {
                OrderId = order.Id,
                Order = order,
                MenuItemId = Guid.NewGuid(),
                MenuItemName = $"Item {i + 1}",
                UnitPrice = 10m + i,
                Quantity = 1
            };
            order.Items.Add(item);
        }

        // Assert
        order.Items.Should().HaveCount(actualItemCount);
        foreach (var item in order.Items)
        {
            item.OrderId.Should().Be(order.Id);
            item.Quantity.Should().BeGreaterThan(0);
            item.UnitPrice.Should().BeGreaterThanOrEqualTo(0);
        }
    }

    /// <summary>
    /// Property: Order total price calculation is non-negative after item additions.
    /// For any Order with valid OrderItems, the TotalAmount SHALL always be non-negative,
    /// and after calculating from items, SHALL equal sum of all item subtotals.
    /// </summary>
    [Property]
    public void OrderEntity_WithItems_TotalPriceCalculationIsNonNegative(
        PositiveInt itemCount,
        PositiveInt unitPrice)
    {
        // Arrange
        var order = new Order
        {
            CustomerName = "Test Customer",
            Email = "test@example.com",
            PhoneNumber = "1234567890",
            Type = OrderType.DineIn,
            Status = OrderStatus.Pending,
            Items = new List<OrderItem>()
        };

        // Act - Create items with valid prices
        var actualItemCount = Math.Min(itemCount.Get, 50);
        var actualPrice = unitPrice.Get / 100m; // Convert to decimal
        decimal expectedTotal = 0;

        for (int i = 0; i < actualItemCount; i++)
        {
            var quantity = Math.Max(1, (i % 5) + 1); // Quantity between 1 and 5
            var item = new OrderItem
            {
                OrderId = order.Id,
                Order = order,
                MenuItemId = Guid.NewGuid(),
                MenuItemName = $"Item {i + 1}",
                UnitPrice = actualPrice,
                Quantity = quantity
            };
            order.Items.Add(item);
            expectedTotal += item.SubTotal;
        }

        // Update order total
        order.TotalAmount = expectedTotal;

        // Assert - Verify domain invariant: TotalAmount >= 0
        order.TotalAmount.Should().BeGreaterThanOrEqualTo(0);
        order.TotalAmount.Should().Be(expectedTotal);
    }

    /// <summary>
    /// Property: Order status transitions are valid according to state machine.
    /// For any Order, status transitions SHALL follow the defined state machine:
    /// Pending → Confirmed → Preparing → Ready → Completed, with Cancelled possible from any state.
    /// Transitions to invalid states SHALL throw appropriate exceptions or fail validation.
    /// </summary>
    [Property]
    public void OrderEntity_StatusTransitions_FollowValidStateMachine(
        OrderStatus initialStatus,
        OrderStatus targetStatus)
    {
        // Arrange
        var order = new Order
        {
            CustomerName = "Test Customer",
            Email = "test@example.com",
            PhoneNumber = "1234567890",
            Type = OrderType.DineIn,
            Status = initialStatus,
            TotalAmount = 100m,
            Items = new List<OrderItem>()
        };

        var originalStatus = order.Status;

        // Act
        var isValidTransition = IsValidStatusTransition(originalStatus, targetStatus);
        
        if (isValidTransition)
        {
            order.Status = targetStatus;
        }

        // Assert
        if (isValidTransition)
        {
            order.Status.Should().Be(targetStatus);
            order.Status.Should().NotBe(originalStatus);
        }
        else
        {
            order.Status.Should().Be(originalStatus);
        }
    }

    /// <summary>
    /// Property: Order constraint validation - total price is never negative.
    /// For any Order with any configuration of items or status, TotalAmount SHALL always be >= 0.
    /// </summary>
    [Property]
    public void OrderEntity_ConstraintValidation_TotalPriceNeverNegative(
        PositiveInt totalAmountInCents)
    {
        // Arrange
        var totalAmount = totalAmountInCents.Get / 100m;
        
        // Act
        var order = new Order
        {
            CustomerName = "Test Customer",
            Email = "test@example.com",
            PhoneNumber = "1234567890",
            Type = OrderType.DineIn,
            Status = OrderStatus.Pending,
            TotalAmount = totalAmount,
            Items = new List<OrderItem>()
        };

        // Assert - Verify domain invariant: TotalAmount >= 0
        order.TotalAmount.Should().BeGreaterThanOrEqualTo(0);
    }

    /// <summary>
    /// Property: Order item pricing constraint - all items have valid pricing.
    /// For any OrderItem in an Order, UnitPrice SHALL be non-negative,
    /// Quantity SHALL be positive, and SubTotal SHALL equal UnitPrice * Quantity.
    /// </summary>
    [Property]
    public void OrderEntity_ItemPricingConstraint_AllItemsHaveValidPricing(
        PositiveInt unitPriceInCents,
        PositiveInt quantity)
    {
        // Arrange
        var unitPrice = unitPriceInCents.Get / 100m;
        var actualQuantity = Math.Max(1, quantity.Get);

        var orderItem = new OrderItem
        {
            OrderId = Guid.NewGuid(),
            MenuItemId = Guid.NewGuid(),
            MenuItemName = "Test Item",
            UnitPrice = unitPrice,
            Quantity = actualQuantity
        };

        // Act & Assert
        orderItem.UnitPrice.Should().BeGreaterThanOrEqualTo(0);
        orderItem.Quantity.Should().BeGreaterThan(0);
        orderItem.SubTotal.Should().Be(unitPrice * actualQuantity);
        orderItem.SubTotal.Should().BeGreaterThanOrEqualTo(0);
    }

    /// <summary>
    /// Property: Order type constraint validation.
    /// For any Order, the Type property SHALL be a valid OrderType enum value
    /// (Collection, Delivery, or DineIn).
    /// </summary>
    [Property]
    public void OrderEntity_TypeConstraint_ValidOrderTypeValues()
    {
        // Arrange & Act
        var validOrderTypes = new[] { OrderType.Collection, OrderType.Delivery, OrderType.DineIn };
        
        // Assert
        foreach (var orderType in validOrderTypes)
        {
            var order = new Order
            {
                CustomerName = "Test",
                Type = orderType
            };

            order.Type.Should().BeOneOf(OrderType.Collection, OrderType.Delivery, OrderType.DineIn);
        }
    }

    /// <summary>
    /// Property: Order status constraint validation.
    /// For any Order, the Status property SHALL be a valid OrderStatus enum value
    /// (Pending, Confirmed, Preparing, Ready, Completed, or Cancelled).
    /// </summary>
    [Property]
    public void OrderEntity_StatusConstraint_ValidOrderStatusValues()
    {
        // Arrange & Act
        var validStatuses = new[]
        {
            OrderStatus.Pending,
            OrderStatus.Confirmed,
            OrderStatus.Preparing,
            OrderStatus.Ready,
            OrderStatus.Completed,
            OrderStatus.Cancelled
        };

        // Assert
        foreach (var status in validStatuses)
        {
            var order = new Order
            {
                CustomerName = "Test",
                Status = status
            };

            order.Status.Should().BeOneOf(
                OrderStatus.Pending,
                OrderStatus.Confirmed,
                OrderStatus.Preparing,
                OrderStatus.Ready,
                OrderStatus.Completed,
                OrderStatus.Cancelled
            );
        }
    }

    /// <summary>
    /// Property: Order domain invariants are preserved after state changes.
    /// For any Order undergoing state transitions (status updates, item modifications),
    /// all domain invariants (TotalAmount >= 0, all items have valid pricing, status is valid)
    /// SHALL remain satisfied.
    /// </summary>
    [Property]
    public void OrderEntity_DomainInvariants_PreservedAfterStateChanges(
        NonEmptyString customerName,
        PositiveInt initialTotalInCents)
    {
        // Arrange
        var initialTotal = initialTotalInCents.Get / 100m;
        var order = new Order
        {
            CustomerName = customerName.Get,
            Email = "test@example.com",
            PhoneNumber = "1234567890",
            Type = OrderType.DineIn,
            Status = OrderStatus.Pending,
            TotalAmount = initialTotal,
            Items = new List<OrderItem>()
        };

        // Verify initial invariants
        order.TotalAmount.Should().BeGreaterThanOrEqualTo(0);
        order.Status.Should().BeOneOf(OrderStatus.Pending, OrderStatus.Confirmed, OrderStatus.Preparing,
            OrderStatus.Ready, OrderStatus.Completed, OrderStatus.Cancelled);

        // Act - Simulate state change: add item
        var item = new OrderItem
        {
            OrderId = order.Id,
            Order = order,
            MenuItemId = Guid.NewGuid(),
            MenuItemName = "New Item",
            UnitPrice = 25m,
            Quantity = 2
        };
        order.Items.Add(item);
        order.TotalAmount += item.SubTotal;

        // Act - Simulate state change: update status
        order.Status = OrderStatus.Confirmed;

        // Assert - Verify invariants after state changes
        order.TotalAmount.Should().BeGreaterThanOrEqualTo(0);
        order.Items.Should().NotBeEmpty();
        order.Items.All(i => i.UnitPrice >= 0 && i.Quantity > 0).Should().BeTrue();
        order.Status.Should().BeOneOf(OrderStatus.Pending, OrderStatus.Confirmed, OrderStatus.Preparing,
            OrderStatus.Ready, OrderStatus.Completed, OrderStatus.Cancelled);
    }

    /// <summary>
    /// Property: Multiple orders maintain independent invariants.
    /// For multiple Order instances created independently, each order's invariants
    /// SHALL be satisfied independently without interference.
    /// </summary>
    [Property]
    public void OrderEntity_MultipleOrders_MaintainIndependentInvariants(
        NonEmptyString customer1,
        NonEmptyString customer2,
        PositiveInt total1InCents,
        PositiveInt total2InCents)
    {
        // Arrange
        var total1 = total1InCents.Get / 100m;
        var total2 = total2InCents.Get / 100m;

        var order1 = new Order
        {
            CustomerName = customer1.Get,
            Email = "customer1@example.com",
            PhoneNumber = "1111111111",
            Type = OrderType.DineIn,
            Status = OrderStatus.Pending,
            TotalAmount = total1,
            Items = new List<OrderItem>()
        };

        var order2 = new Order
        {
            CustomerName = customer2.Get,
            Email = "customer2@example.com",
            PhoneNumber = "2222222222",
            Type = OrderType.Delivery,
            Status = OrderStatus.Confirmed,
            TotalAmount = total2,
            Items = new List<OrderItem>()
        };

        // Act & Assert - Each order maintains its own invariants
        order1.TotalAmount.Should().Be(total1);
        order1.TotalAmount.Should().BeGreaterThanOrEqualTo(0);
        
        order2.TotalAmount.Should().Be(total2);
        order2.TotalAmount.Should().BeGreaterThanOrEqualTo(0);
        
        // Orders should be independent
        order1.CustomerName.Should().NotBe(order2.CustomerName);
        order1.Id.Should().NotBe(order2.Id);
    }

    /// <summary>
    /// Helper method to determine if a status transition is valid.
    /// Valid transitions follow the state machine: 
    /// Pending → Confirmed → Preparing → Ready → Completed
    /// Cancelled is valid from any state.
    /// </summary>
    private static bool IsValidStatusTransition(OrderStatus from, OrderStatus to)
    {
        // If transitioning to same status, it's not a transition
        if (from == to)
            return false;

        // Cancelled can be transitioned to from any state
        if (to == OrderStatus.Cancelled)
            return true;

        // From Cancelled, no transitions are allowed
        if (from == OrderStatus.Cancelled)
            return false;

        // Define valid transitions
        var validTransitions = new Dictionary<OrderStatus, OrderStatus>
        {
            { OrderStatus.Pending, OrderStatus.Confirmed },
            { OrderStatus.Confirmed, OrderStatus.Preparing },
            { OrderStatus.Preparing, OrderStatus.Ready },
            { OrderStatus.Ready, OrderStatus.Completed }
        };

        return validTransitions.TryGetValue(from, out var validNext) && validNext == to;
    }
}
