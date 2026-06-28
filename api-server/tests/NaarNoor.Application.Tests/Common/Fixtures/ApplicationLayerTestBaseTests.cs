using FluentAssertions;
using Moq;
using Xunit;

namespace NaarNoor.Application.Tests.Common.Fixtures;

/// <summary>
/// Integration tests for ApplicationLayerTestBase functionality.
/// 
/// Tests that:
/// 1. ApplicationLayerTestBase properly creates mock dependencies
/// 2. Mock factory methods create mocks with correct behavior
/// 3. Assertion helpers for repository calls work correctly
/// 4. Handler assertion methods validate correctly
/// 5. Mock verification methods function as expected
/// 
/// Validates: Requirements 10.5
/// </summary>
public class ApplicationLayerTestBaseTests : ApplicationLayerTestBase
{
    [Fact]
    public void ApplicationLayerTestBase_CreateRepositoryMock_ReturnsStrictMock()
    {
        // Arrange & Act
        var repositoryMock = CreateRepositoryMock<ITestRepository>();

        // Assert
        repositoryMock.Should().NotBeNull();
        repositoryMock.Behavior.Should().Be(MockBehavior.Strict);
    }

    [Fact]
    public void ApplicationLayerTestBase_CreateServiceMock_ReturnsLooseMock()
    {
        // Arrange & Act
        var serviceMock = CreateServiceMock<ITestService>();

        // Assert
        serviceMock.Should().NotBeNull();
        serviceMock.Behavior.Should().Be(MockBehavior.Loose);
    }

    [Fact]
    public void ApplicationLayerTestBase_CreateRepositoryMock_ThrowsOnUnexpectedCalls()
    {
        // Arrange
        var repositoryMock = CreateRepositoryMock<ITestRepository>();

        // Act & Assert - calling a method not set up on strict mock should throw
        var action = () => repositoryMock.Object.DoSomethingAsync().Wait();
        action.Should().Throw<MockException>();
    }

    [Fact]
    public void ApplicationLayerTestBase_CreateRepositoryMock_SetupAndVerify_Works()
    {
        // Arrange
        var repositoryMock = CreateRepositoryMock<ITestRepository>();
        repositoryMock
            .Setup(x => x.DoSomethingAsync())
            .Returns(Task.CompletedTask);

        // Act
        var action = () => repositoryMock.Object.DoSomethingAsync().Wait();

        // Assert
        action.Should().NotThrow();
        repositoryMock.Verify(x => x.DoSomethingAsync(), Times.Once);
    }

    [Fact]
    public void ApplicationLayerTestBase_CreateServiceMock_DefaultBehaviorIsLoose()
    {
        // Arrange
        var serviceMock = CreateServiceMock<ITestService>();

        // Act - calling an unset up method on loose mock should not throw
        var result = serviceMock.Object.GetValue();

        // Assert
        result.Should().Be(default(string)); // Loose mock returns default
    }

    [Fact]
    public void ApplicationLayerTestBase_AssertRepositoryMethodCalled_VerifiesCall()
    {
        // Arrange
        var repositoryMock = CreateRepositoryMock<ITestRepository>();
        repositoryMock
            .Setup(x => x.DoSomethingAsync())
            .Returns(Task.CompletedTask);

        // Act
        repositoryMock.Object.DoSomethingAsync().Wait();

        // Assert - should not throw
        Action action = () => AssertRepositoryMethodCalled(
            repositoryMock,
            x => x.DoSomethingAsync());
        action.Should().NotThrow();
    }

    [Fact]
    public void ApplicationLayerTestBase_AssertRepositoryMethodNotCalled_VerifiesAbsenceOfCall()
    {
        // Arrange
        var repositoryMock = CreateRepositoryMock<ITestRepository>();
        repositoryMock
            .Setup(x => x.DoSomethingAsync())
            .Returns(Task.CompletedTask);

        // Act - don't call the method

        // Assert - should not throw
        Action action = () => AssertRepositoryMethodNotCalled(
            repositoryMock,
            x => x.DoSomethingAsync());
        action.Should().NotThrow();
    }

    [Fact]
    public async Task ApplicationLayerTestBase_AssertHandlerThrowsAsync_VerifiesException()
    {
        // Arrange
        async Task handlerCall()
        {
            await Task.Delay(10);
            throw new InvalidOperationException("Handler failed");
        }

        // Act & Assert
        var action = async () => await AssertHandlerThrowsAsync<InvalidOperationException>(
            handlerCall,
            "failed");
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ApplicationLayerTestBase_AssertHandlerThrowsAsync_WithWrongException_Fails()
    {
        // Arrange
        async Task handlerCall()
        {
            await Task.Delay(10);
            throw new ArgumentException("Wrong error");
        }

        // Act & Assert
        var action = async () => await AssertHandlerThrowsAsync<InvalidOperationException>(
            handlerCall);
        await action.Should().ThrowAsync<Xunit.Sdk.XunitException>();
    }

    [Fact]
    public void ApplicationLayerTestBase_SetupRepositoryMethodSuccess_ConfiguresMock()
    {
        // Arrange
        var repositoryMock = CreateRepositoryMock<ITestRepository>();

        // Act
        SetupRepositoryMethodSuccess(repositoryMock, x => x.DoSomethingAsync());
        repositoryMock.Object.DoSomethingAsync().Wait();

        // Assert
        repositoryMock.Verify(x => x.DoSomethingAsync(), Times.Once);
    }

    [Fact]
    public void ApplicationLayerTestBase_SetupRepositoryMethodReturn_ReturnsValue()
    {
        // Arrange
        var repositoryMock = CreateRepositoryMock<ITestRepository>();
        var expectedValue = 42;

        // Act
        SetupRepositoryMethodReturn(
            repositoryMock,
            x => x.GetValueAsync(),
            expectedValue);

        var result = repositoryMock.Object.GetValueAsync().Result;

        // Assert
        result.Should().Be(expectedValue);
    }

    [Fact]
    public void ApplicationLayerTestBase_SetupRepositoryMethodThrows_ThrowsException()
    {
        // Arrange
        var repositoryMock = CreateRepositoryMock<ITestRepository>();
        var exception = new InvalidOperationException("Repository error");

        // Act
        SetupRepositoryMethodThrows(
            repositoryMock,
            x => x.DoSomethingAsync(),
            exception);

        // Assert
        var action = () => repositoryMock.Object.DoSomethingAsync().Wait();
        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ApplicationLayerTestBase_VerifyAllMocks_SucceedsWithAllExpectedCalls()
    {
        // Arrange
        var repositoryMock = CreateRepositoryMock<ITestRepository>();
        repositoryMock
            .Setup(x => x.DoSomethingAsync())
            .Returns(Task.CompletedTask);

        // Act
        repositoryMock.Object.DoSomethingAsync().Wait();

        // Assert
        var action = () => VerifyAllMocks(repositoryMock);
        action.Should().NotThrow();
    }

    [Fact]
    public void ApplicationLayerTestBase_VerifyAllMocks_FailsWithUnexpectedCalls()
    {
        // Arrange
        var repositoryMock = CreateRepositoryMock<ITestRepository>();
        repositoryMock
            .Setup(x => x.DoSomethingAsync())
            .Returns(Task.CompletedTask);

        // Act - don't call the method, but mark it as expected
        // This test verifies that VerifyAll() would catch this

        // Assert
        var action = () => VerifyAllMocks(repositoryMock);
        action.Should().Throw<MockException>();
    }

    [Fact]
    public void ApplicationLayerTestBase_AssertValidationFailed_WithFailedValidation_Succeeds()
    {
        // Arrange
        var validationResult = new FluentValidation.Results.ValidationResult(
            new[] { new FluentValidation.Results.ValidationFailure("Field", "Error") }
        );

        // Act & Assert
        var action = () => AssertValidationFailed(validationResult);
        action.Should().NotThrow();
    }

    [Fact]
    public void ApplicationLayerTestBase_AssertValidationFailed_WithSuccessfulValidation_Fails()
    {
        // Arrange
        var validationResult = new FluentValidation.Results.ValidationResult();

        // Act & Assert
        var action = () => AssertValidationFailed(validationResult);
        action.Should().Throw<Xunit.Sdk.XunitException>();
    }

    [Fact]
    public void ApplicationLayerTestBase_AssertValidationFailed_WithFieldName_VerifiesField()
    {
        // Arrange
        var validationResult = new FluentValidation.Results.ValidationResult(
            new[] { new FluentValidation.Results.ValidationFailure("TestField", "Error") }
        );

        // Act & Assert
        var action = () => AssertValidationFailed(validationResult, "TestField");
        action.Should().NotThrow();
    }

    [Fact]
    public void ApplicationLayerTestBase_AssertValidationFailed_WithWrongFieldName_Fails()
    {
        // Arrange
        var validationResult = new FluentValidation.Results.ValidationResult(
            new[] { new FluentValidation.Results.ValidationFailure("Field1", "Error") }
        );

        // Act & Assert
        var action = () => AssertValidationFailed(validationResult, "Field2");
        action.Should().Throw<Xunit.Sdk.XunitException>();
    }

    [Fact]
    public void ApplicationLayerTestBase_AssertValidationSucceeded_WithSuccessfulValidation_Succeeds()
    {
        // Arrange
        var validationResult = new FluentValidation.Results.ValidationResult();

        // Act & Assert
        var action = () => AssertValidationSucceeded(validationResult);
        action.Should().NotThrow();
    }

    [Fact]
    public void ApplicationLayerTestBase_AssertValidationSucceeded_WithFailedValidation_Fails()
    {
        // Arrange
        var validationResult = new FluentValidation.Results.ValidationResult(
            new[] { new FluentValidation.Results.ValidationFailure("Field", "Error") }
        );

        // Act & Assert
        var action = () => AssertValidationSucceeded(validationResult);
        action.Should().Throw<Xunit.Sdk.XunitException>();
    }

    [Fact]
    public void ApplicationLayerTestBase_AllMockFactoryMethodsAreAccessible()
    {
        // Verify that all mock factory methods are available
        var bindingFlags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static;

        var methods = typeof(ApplicationLayerTestBase).GetMethods(bindingFlags);
        
        // Check that methods with these names exist
        methods.Any(m => m.Name == "CreateRepositoryMock").Should().BeTrue();
        methods.Any(m => m.Name == "CreateServiceMock").Should().BeTrue();
        methods.Any(m => m.Name == "AssertRepositoryMethodCalled").Should().BeTrue();
        methods.Any(m => m.Name == "AssertRepositoryMethodNotCalled").Should().BeTrue();
        methods.Any(m => m.Name == "AssertHandlerThrowsAsync").Should().BeTrue();
        methods.Any(m => m.Name == "VerifyAllMocks").Should().BeTrue();
    }
}

/// <summary>
/// Test interface for repository testing.
/// </summary>
public interface ITestRepository
{
    Task DoSomethingAsync();
    Task<int> GetValueAsync();
}

/// <summary>
/// Test interface for service testing.
/// </summary>
public interface ITestService
{
    string GetValue();
}
