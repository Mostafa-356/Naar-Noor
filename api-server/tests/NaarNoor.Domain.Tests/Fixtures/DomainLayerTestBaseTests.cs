using FluentAssertions;
using NaarNoor.Domain.Common;
using Xunit;

namespace NaarNoor.Domain.Tests.Fixtures;

/// <summary>
/// Integration tests for DomainLayerTestBase functionality.
/// 
/// Tests that:
/// 1. DomainLayerTestBase properly provides assertion helpers
/// 2. Exception assertion methods work correctly
/// 3. Entity initialization assertions function as expected
/// 4. Value object immutability assertions are enforced
/// 5. Business rule validation assertions work properly
/// 
/// Validates: Requirements 10.5
/// </summary>
public class DomainLayerTestBaseTests : DomainLayerTestBase
{
    [Fact]
    public void DomainLayerTestBase_AssertExceptionThrown_WithMatchingException_DoesNotThrow()
    {
        // Arrange
        Action action = () => throw new ArgumentException("Test error message");

        // Act & Assert
        Action testAction = () => AssertExceptionThrown<ArgumentException>(action);
        testAction.Should().NotThrow();
    }

    [Fact]
    public void DomainLayerTestBase_AssertExceptionThrown_WithWrongException_Throws()
    {
        // Arrange
        Action action = () => throw new InvalidOperationException("Test error");

        // Act & Assert
        Action testAction = () => AssertExceptionThrown<ArgumentException>(action);
        testAction.Should().Throw<Xunit.Sdk.XunitException>();
    }

    [Fact]
    public void DomainLayerTestBase_AssertExceptionThrown_WithExpectedMessage_Succeeds()
    {
        // Arrange
        var errorMessage = "This is the expected error message";
        Action action = () => throw new ArgumentException(errorMessage);

        // Act & Assert
        Action testAction = () => AssertExceptionThrown<ArgumentException>(action, "expected error");
        testAction.Should().NotThrow();
    }

    [Fact]
    public void DomainLayerTestBase_AssertExceptionThrown_WithUnexpectedMessage_Fails()
    {
        // Arrange
        Action action = () => throw new ArgumentException("Different message");

        // Act & Assert
        Action testAction = () => AssertExceptionThrown<ArgumentException>(action, "expected error");
        testAction.Should().Throw<Xunit.Sdk.XunitException>();
    }

    [Fact]
    public async Task DomainLayerTestBase_AssertExceptionThrownAsync_WithAsyncException_Succeeds()
    {
        // Arrange
        var asyncAction = async () => 
        {
            await Task.Delay(10);
            throw new ArgumentException("Async error");
        };

        // Act & Assert
        var testAction = async () => await AssertExceptionThrownAsync<ArgumentException>(asyncAction);
        await testAction.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DomainLayerTestBase_AssertExceptionThrownAsync_WithWrongException_Fails()
    {
        // Arrange
        var asyncAction = async () => 
        {
            await Task.Delay(10);
            throw new InvalidOperationException("Async error");
        };

        // Act & Assert
        var testAction = async () => await AssertExceptionThrownAsync<ArgumentException>(asyncAction);
        await testAction.Should().ThrowAsync<Xunit.Sdk.XunitException>();
    }

    [Fact]
    public void DomainLayerTestBase_AssertBusinessRuleEnforced_WithMatchingException_Succeeds()
    {
        // Arrange
        Action action = () => throw new ArgumentException("Business rule violation");

        // Act & Assert
        Action testAction = () => AssertBusinessRuleEnforced<ArgumentException>(action);
        testAction.Should().NotThrow();
    }

    [Fact]
    public void DomainLayerTestBase_AssertBusinessRuleEnforced_WithRuleDescription_Matches()
    {
        // Arrange
        Action action = () => throw new ArgumentException("Price must be greater than zero");

        // Act & Assert
        Action testAction = () => AssertBusinessRuleEnforced<ArgumentException>(action, "greater than zero");
        testAction.Should().NotThrow();
    }

    [Fact]
    public void DomainLayerTestBase_AssertValueObjectImmutable_WithPublicSetter_IdentifiesPropertyAsNotImmutable()
    {
        // Arrange
        var mutableObject = new MutableTestObject();

        // Act & Assert
        var testAction = () => AssertValueObjectImmutable(mutableObject);
        // This should throw because MutableTestObject has a public setter
        testAction.Should().Throw<Xunit.Sdk.XunitException>();
    }

    [Fact]
    public void DomainLayerTestBase_AssertValueObjectImmutable_WithoutPublicSetter_VerifiesImmutability()
    {
        // Arrange
        var immutableObject = new ImmutableTestObject("test-value");

        // Act & Assert
        var testAction = () => AssertValueObjectImmutable(immutableObject);
        // Should not throw because ImmutableTestObject has no public setters
        testAction.Should().NotThrow();
    }

    [Fact]
    public void DomainLayerTestBase_AssertValueObjectsEqual_WithEqualObjects_Succeeds()
    {
        // Arrange
        var obj1 = new ImmutableTestObject("same-value");
        var obj2 = new ImmutableTestObject("same-value");

        // Act & Assert
        var testAction = () => AssertValueObjectsEqual(obj1, obj2);
        // This may throw depending on IEquatable implementation
        // but we're testing that the assertion helper is callable
        testAction.Should().NotThrow();
    }

    [Fact]
    public void DomainLayerTestBase_AssertValueObjectsNotEqual_WithDifferentObjects_Succeeds()
    {
        // Arrange
        var obj1 = new ImmutableTestObject("value1");
        var obj2 = new ImmutableTestObject("value2");

        // Act & Assert
        var testAction = () => AssertValueObjectsNotEqual(obj1, obj2);
        testAction.Should().NotThrow();
    }

    [Fact]
    public void DomainLayerTestBase_AssertPropertyIsReadOnly_WithReadOnlyProperty_Succeeds()
    {
        // Arrange & Act & Assert
        var testAction = () => AssertPropertyIsReadOnly<ImmutableTestObject>("Value");
        testAction.Should().NotThrow();
    }

    [Fact]
    public void DomainLayerTestBase_AssertPropertyIsReadOnly_WithWritableProperty_Fails()
    {
        // Arrange & Act & Assert
        var testAction = () => AssertPropertyIsReadOnly<MutableTestObject>("Value");
        testAction.Should().Throw<Xunit.Sdk.XunitException>();
    }

    [Fact]
    public void DomainLayerTestBase_AssertPropertyIsReadOnly_WithNonexistentProperty_Fails()
    {
        // Arrange & Act & Assert
        var testAction = () => AssertPropertyIsReadOnly<ImmutableTestObject>("NonexistentProperty");
        testAction.Should().Throw<Xunit.Sdk.XunitException>();
    }

    [Fact]
    public void DomainLayerTestBase_AssertEntityPropertyValue_WithMatchingValue_Succeeds()
    {
        // Arrange
        var obj = new ImmutableTestObject("test-value");

        // Act & Assert
        var testAction = () => AssertEntityPropertyValue(obj, "Value", "test-value");
        testAction.Should().NotThrow();
    }

    [Fact]
    public void DomainLayerTestBase_AssertEntityPropertyValue_WithDifferentValue_Fails()
    {
        // Arrange
        var obj = new ImmutableTestObject("test-value");

        // Act & Assert
        var testAction = () => AssertEntityPropertyValue(obj, "Value", "different-value");
        testAction.Should().Throw<Xunit.Sdk.XunitException>();
    }

    [Fact]
    public void DomainLayerTestBase_AssertEntitiesEqual_WithEquivalentEntities_Succeeds()
    {
        // Arrange
        var obj1 = new ImmutableTestObject("same-value");
        var obj2 = new ImmutableTestObject("same-value");

        // Act & Assert
        var testAction = () => AssertEntitiesEqual(obj1, obj2);
        testAction.Should().NotThrow();
    }

    [Fact]
    public void DomainLayerTestBase_AllAssertionHelpersAreAccessible()
    {
        // Verify that all key assertion helper methods are available
        // These methods are protected and need the correct binding flags
        
        var bindingFlags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        
        typeof(DomainLayerTestBase).GetMethod("AssertExceptionThrown", bindingFlags).Should().NotBeNull();
        typeof(DomainLayerTestBase).GetMethod("AssertExceptionThrownAsync", bindingFlags).Should().NotBeNull();
        typeof(DomainLayerTestBase).GetMethod("AssertValueObjectImmutable", bindingFlags).Should().NotBeNull();
        typeof(DomainLayerTestBase).GetMethod("AssertValueObjectsEqual", bindingFlags).Should().NotBeNull();
        typeof(DomainLayerTestBase).GetMethod("AssertValueObjectsNotEqual", bindingFlags).Should().NotBeNull();
        typeof(DomainLayerTestBase).GetMethod("AssertPropertyIsReadOnly", bindingFlags).Should().NotBeNull();
        typeof(DomainLayerTestBase).GetMethod("AssertEntityPropertyValue", bindingFlags).Should().NotBeNull();
        typeof(DomainLayerTestBase).GetMethod("AssertEntitiesEqual", bindingFlags).Should().NotBeNull();
        typeof(DomainLayerTestBase).GetMethod("AssertBusinessRuleEnforced", bindingFlags).Should().NotBeNull();
    }
}

/// <summary>
/// Test helper: Mutable object with public setter (for testing immutability detection).
/// </summary>
public class MutableTestObject : IEquatable<MutableTestObject>
{
    public string Value { get; set; } = "";

    public MutableTestObject() { }

    public MutableTestObject(string value)
    {
        Value = value;
    }

    public bool Equals(MutableTestObject? other)
    {
        return other != null && Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as MutableTestObject);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}

/// <summary>
/// Test helper: Immutable object with read-only property (for testing immutability verification).
/// </summary>
public class ImmutableTestObject : IEquatable<ImmutableTestObject>
{
    public string Value { get; }

    public ImmutableTestObject(string value)
    {
        Value = value;
    }

    public bool Equals(ImmutableTestObject? other)
    {
        return other != null && Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as ImmutableTestObject);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}
