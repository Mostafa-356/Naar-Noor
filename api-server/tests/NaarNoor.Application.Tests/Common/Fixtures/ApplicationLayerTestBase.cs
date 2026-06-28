using FluentAssertions;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace NaarNoor.Application.Tests.Common.Fixtures;

/// <summary>
/// Base class for application layer unit tests. Provides common setup, mock factory injection,
/// and assertion helpers for testing command and query handlers in isolation.
/// </summary>
/// <remarks>
/// Validates: Requirements 10.5, 10.6, 11.1
/// 
/// This base class:
/// - Provides centralized mock object creation via factory methods
/// - Enables consistent mock configuration across all application layer tests
/// - Supplies helpers for verifying handler behavior and repository calls
/// - Reduces boilerplate in individual test classes
/// </remarks>
public abstract class ApplicationLayerTestBase
{
    /// <summary>
    /// Creates a strictly-verified mock of a repository interface.
    /// Strict mocks throw exceptions on unexpected calls, helping catch over-mocking.
    /// </summary>
    protected static Mock<TRepository> CreateRepositoryMock<TRepository>() where TRepository : class
    {
        return new Mock<TRepository>(MockBehavior.Strict);
    }

    /// <summary>
    /// Creates a loose mock of a service interface (default behavior for calls not explicitly set up).
    /// Useful for service dependencies that are called conditionally.
    /// </summary>
    protected static Mock<TService> CreateServiceMock<TService>() where TService : class
    {
        return new Mock<TService>(MockBehavior.Loose);
    }

    /// <summary>
    /// Helper to verify that a repository method was called exactly once with any parameters.
    /// </summary>
    protected static void AssertRepositoryMethodCalled<TRepository, TArg>(
        Mock<TRepository> repositoryMock,
        Expression<Func<TRepository, Task>> methodExpression) where TRepository : class
    {
        repositoryMock.Verify(methodExpression, Times.Once);
    }

    /// <summary>
    /// Helper to verify that a repository method was NOT called.
    /// </summary>
    protected static void AssertRepositoryMethodNotCalled<TRepository>(
        Mock<TRepository> repositoryMock,
        Expression<Func<TRepository, Task>> methodExpression) where TRepository : class
    {
        repositoryMock.Verify(methodExpression, Times.Never);
    }

    /// <summary>
    /// Helper to verify a non-async repository method was called exactly once.
    /// </summary>
    protected static void AssertRepositoryMethodCalled<TRepository, TResult>(
        Mock<TRepository> repositoryMock,
        Expression<Func<TRepository, TResult>> methodExpression) where TRepository : class
    {
        repositoryMock.Verify(methodExpression, Times.Once);
    }

    /// <summary>
    /// Helper to assert that a handler call results in an exception with expected characteristics.
    /// </summary>
    protected static async Task AssertHandlerThrowsAsync<TException>(
        Func<Task> handlerCall,
        string? expectedMessageContains = null) where TException : Exception
    {
        await handlerCall.Should().ThrowAsync<TException>()
            .Where(ex => expectedMessageContains == null || ex.Message.Contains(expectedMessageContains));
    }

    /// <summary>
    /// Helper to verify that all configured mocks were used as expected (no unexpected calls).
    /// </summary>
    protected static void VerifyAllMocks(params Mock[] mocks)
    {
        foreach (var mock in mocks)
        {
            mock.VerifyAll();
        }
    }

    /// <summary>
    /// Helper to set up a repository mock to return a successful async result.
    /// </summary>
    protected static void SetupRepositoryMethodSuccess<TRepository>(
        Mock<TRepository> repositoryMock,
        Expression<Func<TRepository, Task>> methodExpression) where TRepository : class
    {
        repositoryMock
            .Setup(methodExpression)
            .Returns(Task.CompletedTask);
    }

    /// <summary>
    /// Helper to set up a repository mock to return a value asynchronously.
    /// </summary>
    protected static void SetupRepositoryMethodReturn<TRepository, TResult>(
        Mock<TRepository> repositoryMock,
        Expression<Func<TRepository, Task<TResult>>> methodExpression,
        TResult returnValue) where TRepository : class
    {
        repositoryMock
            .Setup(methodExpression)
            .ReturnsAsync(returnValue);
    }

    /// <summary>
    /// Helper to set up a repository mock to throw an exception.
    /// </summary>
    protected static void SetupRepositoryMethodThrows<TRepository, TException>(
        Mock<TRepository> repositoryMock,
        Expression<Func<TRepository, Task>> methodExpression,
        TException exception) where TRepository : class where TException : Exception
    {
        repositoryMock
            .Setup(methodExpression)
            .ThrowsAsync(exception);
    }

    /// <summary>
    /// Helper to create a consistent command validation assertion.
    /// </summary>
    protected static void AssertValidationFailed(FluentValidation.Results.ValidationResult result, string? fieldName = null)
    {
        result.IsValid.Should().BeFalse("Validation should have failed");
        result.Errors.Should().NotBeEmpty("Validation errors should be present");

        if (fieldName != null)
        {
            result.Errors.Should().Contain(e => e.PropertyName == fieldName);
        }
    }

    /// <summary>
    /// Helper to assert that validation succeeded.
    /// </summary>
    protected static void AssertValidationSucceeded(FluentValidation.Results.ValidationResult result)
    {
        result.IsValid.Should().BeTrue("Validation should have succeeded");
        result.Errors.Should().BeEmpty("No validation errors should be present");
    }
}
