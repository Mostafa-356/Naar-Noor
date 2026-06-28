using FluentAssertions;
using NaarNoor.Domain.Common;
using Xunit;

namespace NaarNoor.Domain.Tests.Fixtures;

/// <summary>
/// Base class for domain layer unit tests. Provides common setup and assertion helpers
/// for testing domain entities and value objects in isolation.
/// </summary>
/// <remarks>
/// Validates: Requirements 10.5, 10.6, 11.1
/// </remarks>
public abstract class DomainLayerTestBase
{
    /// <summary>
    /// Asserts that an action throws an exception of the specified type with the expected message.
    /// </summary>
    protected void AssertExceptionThrown<TException>(Action action, string? expectedMessageContains = null) where TException : Exception
    {
        action.Should().Throw<TException>()
            .Where(ex => expectedMessageContains == null || ex.Message.Contains(expectedMessageContains));
    }

    /// <summary>
    /// Asserts that an async action throws an exception of the specified type with the expected message.
    /// </summary>
    protected async Task AssertExceptionThrownAsync<TException>(Func<Task> action, string? expectedMessageContains = null) where TException : Exception
    {
        await action.Should().ThrowAsync<TException>()
            .Where(ex => expectedMessageContains == null || ex.Message.Contains(expectedMessageContains));
    }

    /// <summary>
    /// Helper to validate that an entity was properly initialized with expected values.
    /// </summary>
    protected void AssertEntityInitialized<TEntity>(
        TEntity entity,
        Guid? expectedId = null,
        DateTime? expectedCreatedAt = null) where TEntity : BaseEntity
    {
        entity.Should().NotBeNull();
        entity.Id.Should().NotBe(Guid.Empty);

        if (expectedId.HasValue)
        {
            entity.Id.Should().Be(expectedId.Value);
        }

        if (expectedCreatedAt.HasValue)
        {
            entity.CreatedAt.Should().BeCloseTo(expectedCreatedAt.Value, TimeSpan.FromSeconds(1));
        }
        else
        {
            entity.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }
    }

    /// <summary>
    /// Helper to assert that a value object has immutability (properties cannot be changed after construction).
    /// This is typically verified through reflection and property analysis.
    /// </summary>
    protected void AssertValueObjectImmutable<TValueObject>(TValueObject valueObject) where TValueObject : class
    {
        var properties = typeof(TValueObject).GetProperties();
        
        foreach (var property in properties)
        {
            // Value objects should have no public setters or only private setters
            var setter = property.GetSetMethod();
            setter.Should().BeNull("Value objects should be immutable with no public setters");
        }
    }

    /// <summary>
    /// Helper to assert that two value objects are equal based on their value.
    /// </summary>
    protected void AssertValueObjectsEqual<TValueObject>(TValueObject obj1, TValueObject obj2) where TValueObject : class, IEquatable<TValueObject>
    {
        obj1.Should().Be(obj2);
        obj1.Equals(obj2).Should().BeTrue();
    }

    /// <summary>
    /// Helper to assert that two value objects are not equal.
    /// </summary>
    protected void AssertValueObjectsNotEqual<TValueObject>(TValueObject obj1, TValueObject obj2) where TValueObject : class, IEquatable<TValueObject>
    {
        obj1.Should().NotBe(obj2);
        obj1.Equals(obj2).Should().BeFalse();
    }

    /// <summary>
    /// Helper to assert that a business rule constraint is enforced (throws an exception).
    /// </summary>
    protected void AssertBusinessRuleEnforced<TException>(Action action, string? ruleDescription = null) where TException : Exception
    {
        action.Should().Throw<TException>()
            .Where(ex => ruleDescription == null || ex.Message.Contains(ruleDescription));
    }

    /// <summary>
    /// Helper to assert that properties cannot be set (immutability).
    /// Validates that a property has no public setter.
    /// </summary>
    protected void AssertPropertyIsReadOnly<TEntity>(string propertyName) where TEntity : class
    {
        var property = typeof(TEntity).GetProperty(propertyName);
        property.Should().NotBeNull($"Property {propertyName} should exist");

        var setter = property!.GetSetMethod();
        setter.Should().BeNull("Property should be read-only");
    }

    /// <summary>
    /// Helper to assert equality of two entities by their core properties.
    /// Useful for value object and entity equality testing.
    /// </summary>
    protected void AssertEntitiesEqual<TEntity>(TEntity expected, TEntity actual) where TEntity : class
    {
        expected.Should().BeEquivalentTo(actual);
    }

    /// <summary>
    /// Helper to assert that an entity property has a specific value.
    /// </summary>
    protected void AssertEntityPropertyValue<TEntity, TProperty>(
        TEntity entity,
        string propertyName,
        TProperty expectedValue) where TEntity : class
    {
        var property = typeof(TEntity).GetProperty(propertyName);
        property.Should().NotBeNull($"Property {propertyName} should exist");

        var value = property!.GetValue(entity);
        value.Should().Be(expectedValue);
    }
}
