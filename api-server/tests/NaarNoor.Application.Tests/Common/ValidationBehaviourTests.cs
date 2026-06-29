using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using NaarNoor.Application.Common.Behaviours;
using Xunit;

namespace NaarNoor.Application.Tests.Common;

public record ValidationTestRequest(string Value) : IRequest<string>;

public class ValidationBehaviourTests
{
    [Fact]
    public async Task Handle_WithNoValidators_CallsNext()
    {
        var behaviour = new ValidationBehaviour<ValidationTestRequest, string>(Enumerable.Empty<IValidator<ValidationTestRequest>>());
        var nextCalled = false;
        RequestHandlerDelegate<string> next = () => { nextCalled = true; return Task.FromResult("ok"); };

        var result = await behaviour.Handle(new ValidationTestRequest("data"), next, CancellationToken.None);

        nextCalled.Should().BeTrue();
        result.Should().Be("ok");
    }

    [Fact]
    public async Task Handle_WithPassingValidator_CallsNext()
    {
        var validator = new Mock<IValidator<ValidationTestRequest>>();
        validator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<ValidationTestRequest>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new ValidationResult());

        var behaviour = new ValidationBehaviour<ValidationTestRequest, string>(new[] { validator.Object });
        var nextCalled = false;
        RequestHandlerDelegate<string> next = () => { nextCalled = true; return Task.FromResult("passed"); };

        var result = await behaviour.Handle(new ValidationTestRequest("valid"), next, CancellationToken.None);

        nextCalled.Should().BeTrue();
        result.Should().Be("passed");
    }

    [Fact]
    public async Task Handle_WithFailingValidator_ThrowsValidationException()
    {
        var failures = new List<ValidationFailure>
        {
            new ValidationFailure("Value", "Value is required"),
            new ValidationFailure("Value", "Value too short")
        };
        var validator = new Mock<IValidator<ValidationTestRequest>>();
        validator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<ValidationTestRequest>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new ValidationResult(failures));

        var behaviour = new ValidationBehaviour<ValidationTestRequest, string>(new[] { validator.Object });
        var nextCalled = false;
        RequestHandlerDelegate<string> next = () => { nextCalled = true; return Task.FromResult("should not reach"); };

        Func<Task> act = () => behaviour.Handle(new ValidationTestRequest(""), next, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        nextCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithMultipleValidators_AllAreRun()
    {
        var validator1 = new Mock<IValidator<ValidationTestRequest>>();
        validator1.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<ValidationTestRequest>>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(new ValidationResult());

        var validator2 = new Mock<IValidator<ValidationTestRequest>>();
        validator2.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<ValidationTestRequest>>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(new ValidationResult());

        var behaviour = new ValidationBehaviour<ValidationTestRequest, string>(new[] { validator1.Object, validator2.Object });
        RequestHandlerDelegate<string> next = () => Task.FromResult("ok");

        await behaviour.Handle(new ValidationTestRequest("data"), next, CancellationToken.None);

        validator1.Verify(v => v.ValidateAsync(It.IsAny<ValidationContext<ValidationTestRequest>>(), It.IsAny<CancellationToken>()), Times.Once);
        validator2.Verify(v => v.ValidateAsync(It.IsAny<ValidationContext<ValidationTestRequest>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidationException_ContainsAllFailures()
    {
        var failures = new List<ValidationFailure>
        {
            new ValidationFailure("Field1", "Error 1"),
            new ValidationFailure("Field2", "Error 2")
        };
        var validator = new Mock<IValidator<ValidationTestRequest>>();
        validator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<ValidationTestRequest>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new ValidationResult(failures));

        var behaviour = new ValidationBehaviour<ValidationTestRequest, string>(new[] { validator.Object });
        RequestHandlerDelegate<string> next = () => Task.FromResult("x");

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            behaviour.Handle(new ValidationTestRequest(""), next, CancellationToken.None));

        ex.Errors.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WithOnePassOneFailValidator_ThrowsValidationException()
    {
        var passingValidator = new Mock<IValidator<ValidationTestRequest>>();
        passingValidator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<ValidationTestRequest>>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(new ValidationResult());

        var failingValidator = new Mock<IValidator<ValidationTestRequest>>();
        failingValidator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<ValidationTestRequest>>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("Value", "Failed") }));

        var behaviour = new ValidationBehaviour<ValidationTestRequest, string>(new[] { passingValidator.Object, failingValidator.Object });
        RequestHandlerDelegate<string> next = () => Task.FromResult("x");

        Func<Task> act = () => behaviour.Handle(new ValidationTestRequest("test"), next, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
