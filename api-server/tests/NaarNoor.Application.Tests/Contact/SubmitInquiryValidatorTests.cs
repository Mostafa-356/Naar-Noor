using FluentAssertions;
using NaarNoor.Application.Contact.Commands.SubmitInquiry;
using NaarNoor.Application.Tests.Common.Fixtures;
using Xunit;

namespace NaarNoor.Application.Tests.Contact;

public class SubmitInquiryValidatorTests : ApplicationLayerTestBase
{
    private readonly SubmitInquiryCommandValidator _validator = new();

    private static SubmitInquiryCommand ValidCommand() => new(
        Name: "Jane Doe",
        Email: "jane@example.com",
        PhoneNumber: "07700900001",
        Subject: "Table reservation query",
        Message: "I would like to know more about your group booking options."
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
    public async Task Name_Empty_Fails(string? name)
    {
        var cmd = ValidCommand() with { Name = name! };
        var result = await _validator.ValidateAsync(cmd);
        AssertValidationFailed(result, "Name");
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
    [InlineData("")]
    [InlineData(null)]
    public async Task Subject_Empty_Fails(string? subject)
    {
        var cmd = ValidCommand() with { Subject = subject! };
        var result = await _validator.ValidateAsync(cmd);
        AssertValidationFailed(result, "Subject");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Message_Empty_Fails(string? message)
    {
        var cmd = ValidCommand() with { Message = message! };
        var result = await _validator.ValidateAsync(cmd);
        AssertValidationFailed(result, "Message");
    }

    [Fact]
    public async Task Message_TooLong_Fails()
    {
        var cmd = ValidCommand() with { Message = new string('x', 2001) };
        var result = await _validator.ValidateAsync(cmd);
        AssertValidationFailed(result, "Message");
    }

    [Fact]
    public async Task Subject_TooLong_Fails()
    {
        var cmd = ValidCommand() with { Subject = new string('x', 201) };
        var result = await _validator.ValidateAsync(cmd);
        AssertValidationFailed(result, "Subject");
    }

    [Fact]
    public async Task Name_TooLong_Fails()
    {
        var cmd = ValidCommand() with { Name = new string('x', 101) };
        var result = await _validator.ValidateAsync(cmd);
        AssertValidationFailed(result, "Name");
    }

    [Fact]
    public async Task Message_MaxLength_Passes()
    {
        var cmd = ValidCommand() with { Message = new string('x', 2000) };
        var result = await _validator.ValidateAsync(cmd);
        AssertValidationSucceeded(result);
    }
}
