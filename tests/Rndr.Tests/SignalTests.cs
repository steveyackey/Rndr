using Xunit;

namespace Rndr.Tests;

public class SignalTests
{
    [Fact]
    public void Signal_InitialValue_IsAccessible()
    {
        // Arrange & Act
        var signal = new Signal<int>(42);

        // Assert
        Assert.Equal(42, signal.Value);
    }

    [Fact]
    public void Signal_SetSameValue_DoesNotTriggerCallback()
    {
        // Arrange
        var callbackCount = 0;
        var signal = new Signal<int>(42, () => callbackCount++);

        // Act
        signal.Value = 42;

        // Assert
        Assert.Equal(0, callbackCount);
    }

    [Fact]
    public void Signal_SetDifferentValue_TriggersCallback()
    {
        // Arrange
        var callbackCount = 0;
        var signal = new Signal<int>(42, () => callbackCount++);

        // Act
        signal.Value = 100;

        // Assert
        Assert.Equal(1, callbackCount);
        Assert.Equal(100, signal.Value);
    }

    [Fact]
    public void Signal_MultipleChanges_TriggersCallbackEachTime()
    {
        // Arrange
        var callbackCount = 0;
        var signal = new Signal<int>(0, () => callbackCount++);

        // Act
        signal.Value = 1;
        signal.Value = 2;
        signal.Value = 3;

        // Assert
        Assert.Equal(3, callbackCount);
    }

    [Fact]
    public void Signal_NullCallback_DoesNotThrow()
    {
        // Arrange
        var signal = new Signal<string>("test");

        // Act & Assert (should not throw)
        signal.Value = "changed";
        Assert.Equal("changed", signal.Value);
    }

    [Fact]
    public void Signal_ReferenceType_UsesEqualityComparer()
    {
        // Arrange
        var callbackCount = 0;
        var signal = new Signal<string>("hello", () => callbackCount++);

        // Act - set to same value (by content)
        signal.Value = "hello";

        // Assert
        Assert.Equal(0, callbackCount);
    }

    [Fact]
    public void Signal_ImplicitConversion_ReturnsValue()
    {
        // Arrange
        var signal = new Signal<int>(42);

        // Act
        int value = signal;

        // Assert
        Assert.Equal(42, value);
    }

    [Fact]
    public void Signal_UntypedValue_GetAndSet()
    {
        // Arrange
        ISignal signal = new Signal<int>(42);

        // Act
        var untypedValue = signal.UntypedValue;
        signal.UntypedValue = 100;

        // Assert
        Assert.Equal(42, untypedValue);
        Assert.Equal(100, signal.UntypedValue);
    }
}

