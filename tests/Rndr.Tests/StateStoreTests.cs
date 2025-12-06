using Xunit;

namespace Rndr.Tests;

public class StateStoreTests
{
    [Fact]
    public void GetOrCreate_NewKey_CreatesSignal()
    {
        // Arrange
        var store = new InMemoryStateStore();

        // Act
        var signal = store.GetOrCreate("route1", "count", () => 42);

        // Assert
        Assert.NotNull(signal);
        Assert.Equal(42, signal.Value);
    }

    [Fact]
    public void GetOrCreate_ExistingKey_ReturnsSameSignal()
    {
        // Arrange
        var store = new InMemoryStateStore();
        var first = store.GetOrCreate("route1", "count", () => 42);

        // Act
        var second = store.GetOrCreate("route1", "count", () => 100);

        // Assert
        Assert.Same(first, second);
        Assert.Equal(42, second.Value); // Should still be original value
    }

    [Fact]
    public void GetOrCreate_DifferentScopes_CreatesSeparateSignals()
    {
        // Arrange
        var store = new InMemoryStateStore();

        // Act
        var route1Signal = store.GetOrCreate("route1", "count", () => 10);
        var route2Signal = store.GetOrCreate("route2", "count", () => 20);

        // Assert
        Assert.NotSame(route1Signal, route2Signal);
        Assert.Equal(10, route1Signal.Value);
        Assert.Equal(20, route2Signal.Value);
    }

    [Fact]
    public void GetOrCreate_GlobalScope_SharedAcrossRoutes()
    {
        // Arrange
        var store = new InMemoryStateStore();

        // Act
        var first = store.GetOrCreate("global", "username", () => "Alice");
        var second = store.GetOrCreate("global", "username", () => "Bob");

        // Assert
        Assert.Same(first, second);
        Assert.Equal("Alice", second.Value);
    }

    [Fact]
    public void GetOrCreate_DifferentKeys_SameScope_CreatesSeparateSignals()
    {
        // Arrange
        var store = new InMemoryStateStore();

        // Act
        var countSignal = store.GetOrCreate("route1", "count", () => 1);
        var nameSignal = store.GetOrCreate("route1", "name", () => "Test");

        // Assert
        Assert.NotSame(countSignal, nameSignal);
    }

    [Fact]
    public void ClearScope_RemovesOnlyScopedState()
    {
        // Arrange
        var store = new InMemoryStateStore();
        store.GetOrCreate("route1", "count", () => 1);
        store.GetOrCreate("route2", "count", () => 2);
        store.GetOrCreate("global", "shared", () => "shared");

        // Act
        store.ClearScope("route1");

        // Assert - route1 state should be gone, others remain
        var newRoute1 = store.GetOrCreate("route1", "count", () => 100);
        var existingRoute2 = store.GetOrCreate("route2", "count", () => 200);
        var existingGlobal = store.GetOrCreate("global", "shared", () => "new");

        Assert.Equal(100, newRoute1.Value); // New value since old was cleared
        Assert.Equal(2, existingRoute2.Value); // Original value preserved
        Assert.Equal("shared", existingGlobal.Value); // Original value preserved
    }

    [Fact]
    public void Clear_RemovesAllState()
    {
        // Arrange
        var store = new InMemoryStateStore();
        store.GetOrCreate("route1", "count", () => 1);
        store.GetOrCreate("global", "shared", () => "shared");

        // Act
        store.Clear();

        // Assert
        var newRoute1 = store.GetOrCreate("route1", "count", () => 100);
        var newGlobal = store.GetOrCreate("global", "shared", () => "new");

        Assert.Equal(100, newRoute1.Value);
        Assert.Equal("new", newGlobal.Value);
    }

    [Fact]
    public void GetOrCreate_WithOnChanged_PropagatesCallback()
    {
        // Arrange
        var store = new InMemoryStateStore();
        var callbackCount = 0;

        // Act
        var signal = store.GetOrCreate("route1", "count", () => 0, () => callbackCount++);
        signal.Value = 1;

        // Assert
        Assert.Equal(1, callbackCount);
    }

    [Fact]
    public void GetKeysForScope_ReturnsCorrectKeys()
    {
        // Arrange
        var store = new InMemoryStateStore();
        store.GetOrCreate("route1", "count", () => 1);
        store.GetOrCreate("route1", "name", () => "Test");
        store.GetOrCreate("route2", "other", () => 2);

        // Act
        var keys = store.GetKeysForScope("route1").ToList();

        // Assert
        Assert.Equal(2, keys.Count);
        Assert.Contains("count", keys);
        Assert.Contains("name", keys);
    }
}

