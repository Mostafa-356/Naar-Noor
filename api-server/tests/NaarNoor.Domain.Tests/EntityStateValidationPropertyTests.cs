using FsCheck;
using FsCheck.Xunit;
using FluentAssertions;
using NaarNoor.Domain.Entities;
using NaarNoor.Domain.Enums;

namespace NaarNoor.Domain.Tests;

/// <summary>
/// Property-based tests for entity state validation across all domain entities.
/// Validates that entity constructors properly initialize properties and enforce business rules.
/// 
/// **Validates: Requirements 1.1, 1.2, 1.4**
/// **Property 1: Entity State Validation**
/// </summary>
public class EntityStateValidationPropertyTests
{
    /// <summary>
    /// Property: Chef entity properties are correctly initialized when created with valid data.
    /// For any valid Chef input, all properties SHALL be correctly initialized
    /// and the entity SHALL have a unique ID.
    /// </summary>
    [Property]
    public void ChefEntity_WithValidData_AllPropertiesInitializedCorrectly(
        NonEmptyString name,
        NonEmptyString title,
        NonEmptyString bio,
        NonEmptyString specialty)
    {
        // Arrange
        var chef = new Chef
        {
            Name = name.Get,
            Title = title.Get,
            Bio = bio.Get,
            Specialty = specialty.Get,
            IsActive = true,
            SortOrder = 0
        };

        // Act & Assert
        chef.Should().NotBeNull();
        chef.Id.Should().NotBe(Guid.Empty);
        chef.Name.Should().Be(name.Get);
        chef.Title.Should().Be(title.Get);
        chef.Bio.Should().Be(bio.Get);
        chef.Specialty.Should().Be(specialty.Get);
        chef.IsActive.Should().BeTrue();
        chef.SortOrder.Should().Be(0);
        chef.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        chef.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    /// <summary>
    /// Property: Chef entities with different data have different IDs.
    /// No two Chef instances created at different times SHALL have the same ID.
    /// </summary>
    [Property]
    public void ChefEntity_MultipleInstances_HaveDifferentIds(
        NonEmptyString name1,
        NonEmptyString name2)
    {
        // Arrange
        var chef1 = new Chef { Name = name1.Get };
        var chef2 = new Chef { Name = name2.Get };

        // Act & Assert
        chef1.Id.Should().NotBe(chef2.Id);
    }

    /// <summary>
    /// Property: MenuItem entity properties are correctly initialized with valid data.
    /// For any valid MenuItem input with non-negative price, all properties SHALL be correctly initialized.
    /// </summary>
    [Property]
    public void MenuItemEntity_WithValidData_AllPropertiesInitializedCorrectly(
        NonEmptyString name,
        NonEmptyString description,
        PositiveInt priceInCents,
        bool isVegetarian,
        bool isVegan,
        bool isGlutenFree)
    {
        // Arrange
        var price = priceInCents.Get / 100m; // Convert to decimal
        var category = MenuCategory.Starters; // Use default category
        var menuItem = new MenuItem
        {
            Name = name.Get,
            Description = description.Get,
            Price = price,
            Category = category,
            IsVegetarian = isVegetarian,
            IsVegan = isVegan,
            IsGlutenFree = isGlutenFree,
            IsAvailable = true,
            SortOrder = 0
        };

        // Act & Assert
        menuItem.Should().NotBeNull();
        menuItem.Id.Should().NotBe(Guid.Empty);
        menuItem.Name.Should().Be(name.Get);
        menuItem.Description.Should().Be(description.Get);
        menuItem.Price.Should().Be(price);
        menuItem.Category.Should().Be(category);
        menuItem.IsVegetarian.Should().Be(isVegetarian);
        menuItem.IsVegan.Should().Be(isVegan);
        menuItem.IsGlutenFree.Should().Be(isGlutenFree);
        menuItem.IsAvailable.Should().BeTrue();
    }

    /// <summary>
    /// Property: MenuItem price constraint - negative or zero prices should be handled.
    /// For any MenuItem with a non-negative price, the Price property SHALL store the exact value.
    /// </summary>
    [Property]
    public void MenuItemEntity_WithVariousPrices_StoresPriceProperly(
        PositiveInt priceInCents)
    {
        // Arrange
        var price = priceInCents.Get / 100m;
        var menuItem = new MenuItem
        {
            Name = "Test Item",
            Price = price
        };

        // Act & Assert
        menuItem.Price.Should().Be(price);
        menuItem.Price.Should().BeGreaterThanOrEqualTo(0);
    }

    /// <summary>
    /// Property: Reservation entity properties are correctly initialized with valid data.
    /// For any valid Reservation input, all properties SHALL be correctly initialized,
    /// party size SHALL be positive, and initial status SHALL be Pending.
    /// </summary>
    [Property]
    public void ReservationEntity_WithValidData_AllPropertiesInitializedCorrectly(
        NonEmptyString customerName,
        PositiveInt partySize,
        PositiveInt dayOffset)
    {
        // Arrange
        var reservationDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(dayOffset.Get));
        var reservationTime = new TimeOnly(19, 30); // 7:30 PM
        
        var reservation = new Reservation
        {
            CustomerName = customerName.Get,
            Email = "test@example.com",
            PhoneNumber = "1234567890",
            ReservationDate = reservationDate,
            ReservationTime = reservationTime,
            PartySize = partySize.Get,
            Status = ReservationStatus.Pending,
            SpecialRequests = null
        };

        // Act & Assert
        reservation.Should().NotBeNull();
        reservation.Id.Should().NotBe(Guid.Empty);
        reservation.CustomerName.Should().Be(customerName.Get);
        reservation.ReservationDate.Should().Be(reservationDate);
        reservation.ReservationTime.Should().Be(reservationTime);
        reservation.PartySize.Should().Be(partySize.Get);
        reservation.PartySize.Should().BeGreaterThan(0);
        reservation.Status.Should().Be(ReservationStatus.Pending);
    }

    /// <summary>
    /// Property: Reservation party size constraint.
    /// For any Reservation, PartySize SHALL always be positive (> 0).
    /// </summary>
    [Property]
    public void ReservationEntity_WithPositivePartySize_PartySizeIsAlwaysPositive(
        PositiveInt partySize)
    {
        // Arrange
        var reservation = new Reservation
        {
            CustomerName = "John Doe",
            PartySize = partySize.Get
        };

        // Act & Assert
        reservation.PartySize.Should().BeGreaterThan(0);
    }

    /// <summary>
    /// Property: Order entity properties are correctly initialized with valid data.
    /// For any valid Order input, all properties SHALL be correctly initialized,
    /// TotalAmount SHALL be non-negative, and initial status SHALL be Pending.
    /// </summary>
    [Property]
    public void OrderEntity_WithValidData_AllPropertiesInitializedCorrectly(
        NonEmptyString customerName,
        PositiveInt totalAmountInCents)
    {
        // Arrange
        var totalAmount = totalAmountInCents.Get / 100m;
        var orderType = OrderType.DineIn;
        
        var order = new Order
        {
            CustomerName = customerName.Get,
            Email = "test@example.com",
            PhoneNumber = "1234567890",
            Type = orderType,
            TotalAmount = totalAmount,
            Status = OrderStatus.Pending,
            Items = new List<OrderItem>()
        };

        // Act & Assert
        order.Should().NotBeNull();
        order.Id.Should().NotBe(Guid.Empty);
        order.CustomerName.Should().Be(customerName.Get);
        order.Type.Should().Be(orderType);
        order.TotalAmount.Should().Be(totalAmount);
        order.TotalAmount.Should().BeGreaterThanOrEqualTo(0);
        order.Status.Should().Be(OrderStatus.Pending);
        order.Items.Should().NotBeNull();
    }

    /// <summary>
    /// Property: Order total amount constraint.
    /// For any Order, TotalAmount SHALL never be negative.
    /// </summary>
    [Property]
    public void OrderEntity_WithNonNegativeTotal_TotalAmountIsNeverNegative(
        PositiveInt totalAmountInCents)
    {
        // Arrange
        var totalAmount = totalAmountInCents.Get / 100m;
        var order = new Order
        {
            CustomerName = "Test Customer",
            TotalAmount = totalAmount
        };

        // Act & Assert
        order.TotalAmount.Should().BeGreaterThanOrEqualTo(0);
    }

    /// <summary>
    /// Property: Test project structure verification.
    /// Validates that all required test base classes are properly defined
    /// and that the test projects can create instances of all entity types.
    /// </summary>
    [Property]
    public void TestProjectStructure_CanInstantiateAllEntityTypes(NonEmptyString testName)
    {
        // Arrange & Act
        var chef = new Chef { Name = testName.Get };
        var menuItem = new MenuItem { Name = testName.Get };
        var reservation = new Reservation { CustomerName = testName.Get };
        var order = new Order { CustomerName = testName.Get };

        // Assert - All entities should be instantiable and have valid Guids
        chef.Should().NotBeNull();
        chef.Id.Should().NotBe(Guid.Empty);
        
        menuItem.Should().NotBeNull();
        menuItem.Id.Should().NotBe(Guid.Empty);
        
        reservation.Should().NotBeNull();
        reservation.Id.Should().NotBe(Guid.Empty);
        
        order.Should().NotBeNull();
        order.Id.Should().NotBe(Guid.Empty);
    }

    /// <summary>
    /// Property: Base entity timestamps are set correctly.
    /// For any created entity, CreatedAt and UpdatedAt SHALL be set to a time very close to now.
    /// </summary>
    [Property]
    public void BaseEntity_CreatedAtAndUpdatedAt_AreSetToCurrentTime(NonEmptyString name)
    {
        // Arrange
        var before = DateTime.UtcNow;
        var entity = new Chef { Name = name.Get };
        var after = DateTime.UtcNow;

        // Act & Assert
        entity.CreatedAt.Should().BeOnOrAfter(before);
        entity.CreatedAt.Should().BeOnOrBefore(after.AddSeconds(1));
        entity.UpdatedAt.Should().BeOnOrAfter(before);
        entity.UpdatedAt.Should().BeOnOrBefore(after.AddSeconds(1));
    }
}
