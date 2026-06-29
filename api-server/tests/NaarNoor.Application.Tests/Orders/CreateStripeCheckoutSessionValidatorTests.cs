using FluentAssertions;
using NaarNoor.Application.Orders.Commands.CreateStripeCheckoutSession;
using NaarNoor.Application.Tests.Common.Fixtures;
using Xunit;

namespace NaarNoor.Application.Tests.Orders;

public class CreateStripeCheckoutSessionValidatorTests : ApplicationLayerTestBase
{
    private readonly CreateStripeCheckoutSessionCommandValidator _validator = new();

    private static CreateStripeCheckoutSessionCommand ValidDeliveryCommand() => new(
        CustomerName: "Jane Smith",
        Email: "jane@example.com",
        PhoneNumber: "07700900001",
        Notes: null,
        Type: "delivery",
        DeliveryAddress: "12 Himalayan Way, London",
        TableReservationName: null,
        Items: new List<CheckoutOrderItemRequest>
        {
            new(Guid.NewGuid(), "Dal Bhat", 14.95m, 2)
        },
        SuccessUrl: "https://example.com/success",
        CancelUrl: "https://example.com/cancel"
    );

    private static CreateStripeCheckoutSessionCommand ValidCollectionCommand() => new(
        CustomerName: "John Doe",
        Email: "john@example.com",
        PhoneNumber: "07700900002",
        Notes: null,
        Type: "collection",
        DeliveryAddress: null,
        TableReservationName: null,
        Items: new List<CheckoutOrderItemRequest> { new(Guid.NewGuid(), "Momos", 8.95m, 1) },
        SuccessUrl: "https://example.com/success",
        CancelUrl: "https://example.com/cancel"
    );

    private static CreateStripeCheckoutSessionCommand ValidDineInCommand() => new(
        CustomerName: "Sara Magar",
        Email: "sara@example.com",
        PhoneNumber: "07700900003",
        Notes: "Window seat",
        Type: "dine-in",
        DeliveryAddress: null,
        TableReservationName: "Magar party",
        Items: new List<CheckoutOrderItemRequest> { new(Guid.NewGuid(), "Lamb Rogan Josh", 18.95m, 1) },
        SuccessUrl: "https://example.com/success",
        CancelUrl: "https://example.com/cancel"
    );

    [Fact]
    public async Task Valid_DeliveryCommand_Passes()
    {
        var result = await _validator.ValidateAsync(ValidDeliveryCommand());
        AssertValidationSucceeded(result);
    }

    [Fact]
    public async Task Valid_CollectionCommand_Passes()
    {
        var result = await _validator.ValidateAsync(ValidCollectionCommand());
        AssertValidationSucceeded(result);
    }

    [Fact]
    public async Task Valid_DineInCommand_Passes()
    {
        var result = await _validator.ValidateAsync(ValidDineInCommand());
        AssertValidationSucceeded(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("A")]
    [InlineData(null)]
    public async Task CustomerName_TooShortOrEmpty_Fails(string? name)
    {
        var cmd = ValidCollectionCommand() with { CustomerName = name! };
        var result = await _validator.ValidateAsync(cmd);
        AssertValidationFailed(result, "CustomerName");
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData(null)]
    public async Task Email_Invalid_Fails(string? email)
    {
        var cmd = ValidCollectionCommand() with { Email = email! };
        var result = await _validator.ValidateAsync(cmd);
        AssertValidationFailed(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    [InlineData(null)]
    public async Task PhoneNumber_TooShortOrEmpty_Fails(string? phone)
    {
        var cmd = ValidCollectionCommand() with { PhoneNumber = phone! };
        var result = await _validator.ValidateAsync(cmd);
        AssertValidationFailed(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("unknown-type")]
    [InlineData("takeaway")]
    public async Task Type_Invalid_Fails(string type)
    {
        var cmd = ValidCollectionCommand() with { Type = type };
        var result = await _validator.ValidateAsync(cmd);
        AssertValidationFailed(result, "Type");
    }

    [Fact]
    public async Task Delivery_WithoutAddress_Fails()
    {
        var cmd = ValidDeliveryCommand() with { DeliveryAddress = null };
        var result = await _validator.ValidateAsync(cmd);
        AssertValidationFailed(result, "DeliveryAddress");
    }

    [Fact]
    public async Task Delivery_WithShortAddress_Fails()
    {
        var cmd = ValidDeliveryCommand() with { DeliveryAddress = "AB" };
        var result = await _validator.ValidateAsync(cmd);
        AssertValidationFailed(result, "DeliveryAddress");
    }

    [Fact]
    public async Task DineIn_WithoutReservationName_Fails()
    {
        var cmd = ValidDineInCommand() with { TableReservationName = null };
        var result = await _validator.ValidateAsync(cmd);
        AssertValidationFailed(result, "TableReservationName");
    }

    [Fact]
    public async Task Items_Empty_Fails()
    {
        var cmd = ValidCollectionCommand() with { Items = new List<CheckoutOrderItemRequest>() };
        var result = await _validator.ValidateAsync(cmd);
        AssertValidationFailed(result, "Items");
    }

    [Fact]
    public async Task Item_WithZeroQuantity_Fails()
    {
        var cmd = ValidCollectionCommand() with
        {
            Items = new List<CheckoutOrderItemRequest> { new(Guid.NewGuid(), "Dal Bhat", 14.95m, 0) }
        };
        var result = await _validator.ValidateAsync(cmd);
        AssertValidationFailed(result);
    }

    [Fact]
    public async Task Item_WithQuantityOver20_Fails()
    {
        var cmd = ValidCollectionCommand() with
        {
            Items = new List<CheckoutOrderItemRequest> { new(Guid.NewGuid(), "Dal Bhat", 14.95m, 21) }
        };
        var result = await _validator.ValidateAsync(cmd);
        AssertValidationFailed(result);
    }

    [Fact]
    public async Task Item_WithEmptyMenuItemId_Fails()
    {
        var cmd = ValidCollectionCommand() with
        {
            Items = new List<CheckoutOrderItemRequest> { new(Guid.Empty, "Dal Bhat", 14.95m, 1) }
        };
        var result = await _validator.ValidateAsync(cmd);
        AssertValidationFailed(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-url")]
    [InlineData("ftp://example.com")]
    public async Task SuccessUrl_Invalid_Fails(string url)
    {
        var cmd = ValidCollectionCommand() with { SuccessUrl = url };
        var result = await _validator.ValidateAsync(cmd);
        AssertValidationFailed(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-url")]
    public async Task CancelUrl_Invalid_Fails(string url)
    {
        var cmd = ValidCollectionCommand() with { CancelUrl = url };
        var result = await _validator.ValidateAsync(cmd);
        AssertValidationFailed(result);
    }

    [Fact]
    public async Task Collection_WithDeliveryAddress_IsAllowed()
    {
        var cmd = ValidCollectionCommand() with { DeliveryAddress = "12 Main St" };
        var result = await _validator.ValidateAsync(cmd);
        AssertValidationSucceeded(result);
    }
}
