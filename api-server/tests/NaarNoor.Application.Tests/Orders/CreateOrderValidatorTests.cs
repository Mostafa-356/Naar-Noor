using FluentAssertions;
using NaarNoor.Application.Orders.Commands.CreateOrder;
using NaarNoor.Application.Tests.Common.Fixtures;
using Xunit;

namespace NaarNoor.Application.Tests.Orders;

public class CreateOrderValidatorTests : ApplicationLayerTestBase
{
    private readonly CreateOrderCommandValidator _validator = new();

    private static CreateOrderCommand ValidCommand(string type = "collection", string? deliveryAddress = null, string? reservationName = null) =>
        new CreateOrderCommand(
            CustomerName: "John Smith",
            Email: "john@example.com",
            PhoneNumber: "07700900001",
            Notes: null,
            Type: type,
            DeliveryAddress: deliveryAddress,
            TableReservationName: reservationName,
            Items: new List<OrderItemRequest> { new(Guid.NewGuid(), "Dal Bhat", 14.95m, 2) }
        );

    [Fact]
    public async Task ValidCommand_Collection_Passes()
    {
        var result = await _validator.ValidateAsync(ValidCommand("collection"));
        AssertValidationSucceeded(result);
    }

    [Fact]
    public async Task ValidCommand_Delivery_WithAddress_Passes()
    {
        var result = await _validator.ValidateAsync(ValidCommand("delivery", deliveryAddress: "12 Main St, London SW1A 1AA"));
        AssertValidationSucceeded(result);
    }

    [Fact]
    public async Task ValidCommand_DineIn_WithReservationName_Passes()
    {
        var result = await _validator.ValidateAsync(ValidCommand("dine-in", reservationName: "Smith party"));
        AssertValidationSucceeded(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task CustomerName_Empty_Fails(string? name)
    {
        var cmd = ValidCommand() with { CustomerName = name! };
        var result = await _validator.ValidateAsync(cmd);
        AssertValidationFailed(result, "CustomerName");
    }

    [Theory]
    [InlineData("not-email")]
    [InlineData("")]
    [InlineData(null)]
    public async Task Email_Invalid_Fails(string? email)
    {
        var cmd = ValidCommand() with { Email = email! };
        var result = await _validator.ValidateAsync(cmd);
        AssertValidationFailed(result);
    }

    [Theory]
    [InlineData("invalid-type")]
    [InlineData("takeaway")]
    [InlineData("")]
    public async Task Type_Invalid_Fails(string type)
    {
        var cmd = ValidCommand() with { Type = type };
        var result = await _validator.ValidateAsync(cmd);
        AssertValidationFailed(result, "Type");
    }

    [Fact]
    public async Task Items_Empty_Fails()
    {
        var cmd = ValidCommand() with { Items = new List<OrderItemRequest>() };
        var result = await _validator.ValidateAsync(cmd);
        AssertValidationFailed(result, "Items");
    }

    [Fact]
    public async Task Item_ZeroQuantity_Fails()
    {
        var cmd = ValidCommand() with { Items = new List<OrderItemRequest> { new(Guid.NewGuid(), "Dal Bhat", 14.95m, 0) } };
        var result = await _validator.ValidateAsync(cmd);
        AssertValidationFailed(result);
    }

    [Fact]
    public async Task Item_Over20Quantity_Fails()
    {
        var cmd = ValidCommand() with { Items = new List<OrderItemRequest> { new(Guid.NewGuid(), "Dal Bhat", 14.95m, 21) } };
        var result = await _validator.ValidateAsync(cmd);
        AssertValidationFailed(result);
    }

    [Fact]
    public async Task Item_ZeroPrice_Fails()
    {
        var cmd = ValidCommand() with { Items = new List<OrderItemRequest> { new(Guid.NewGuid(), "Dal Bhat", 0m, 1) } };
        var result = await _validator.ValidateAsync(cmd);
        AssertValidationFailed(result);
    }

    [Fact]
    public async Task Delivery_WithoutAddress_Fails()
    {
        var cmd = ValidCommand("delivery", deliveryAddress: null);
        var result = await _validator.ValidateAsync(cmd);
        AssertValidationFailed(result, "DeliveryAddress");
    }

    [Fact]
    public async Task DineIn_WithoutReservationName_Fails()
    {
        var cmd = ValidCommand("dine-in", reservationName: null);
        var result = await _validator.ValidateAsync(cmd);
        AssertValidationFailed(result, "TableReservationName");
    }
}
