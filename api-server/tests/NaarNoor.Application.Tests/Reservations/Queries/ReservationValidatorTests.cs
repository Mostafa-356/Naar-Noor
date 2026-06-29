using FluentAssertions;
using NaarNoor.Application.Reservations.Commands.CreateReservation;
using NaarNoor.Application.Tests.Common.Fixtures;
using Xunit;

namespace NaarNoor.Application.Tests.Reservations.Queries;

public class ReservationValidatorTests : ApplicationLayerTestBase
{
    private readonly CreateReservationCommandValidator _validator = new();

    private static CreateReservationCommand ValidCommand() => new(
        CustomerName: "John Smith",
        Email: "john@example.com",
        PhoneNumber: "07700900001",
        ReservationDate: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
        ReservationTime: "19:00",
        PartySize: 4,
        SpecialRequests: null
    );

    [Fact]
    public async Task ValidCommand_Passes()
    {
        var result = await _validator.ValidateAsync(ValidCommand());
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
    public async Task Email_Invalid_Fails(string email)
    {
        var cmd = ValidCommand() with { Email = email };
        var result = await _validator.ValidateAsync(cmd);
        AssertValidationFailed(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task PhoneNumber_Empty_Fails(string? phone)
    {
        var cmd = ValidCommand() with { PhoneNumber = phone! };
        var result = await _validator.ValidateAsync(cmd);
        AssertValidationFailed(result, "PhoneNumber");
    }

    [Fact]
    public async Task ReservationDate_InPast_Fails()
    {
        var cmd = ValidCommand() with { ReservationDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)) };
        var result = await _validator.ValidateAsync(cmd);
        AssertValidationFailed(result, "ReservationDate");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(21)]
    [InlineData(-1)]
    public async Task PartySize_OutOfRange_Fails(int partySize)
    {
        var cmd = ValidCommand() with { PartySize = partySize };
        var result = await _validator.ValidateAsync(cmd);
        AssertValidationFailed(result, "PartySize");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(20)]
    public async Task PartySize_ValidRange_Passes(int partySize)
    {
        var cmd = ValidCommand() with { PartySize = partySize };
        var result = await _validator.ValidateAsync(cmd);
        AssertValidationSucceeded(result);
    }

    [Fact]
    public async Task ReservationDate_Today_Passes()
    {
        var cmd = ValidCommand() with { ReservationDate = DateOnly.FromDateTime(DateTime.Today) };
        var result = await _validator.ValidateAsync(cmd);
        AssertValidationSucceeded(result);
    }
}
