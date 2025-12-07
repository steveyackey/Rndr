using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Rndr.Extensions;
using Rndr.Navigation;
using Xunit;

namespace Rndr.Tests;

public class ViewContextValidationTests
{
    [Fact]
    public void State_ThrowsWhenKeyIsNull()
    {
        // Arrange
        var context = CreateViewContext();
        string? nullKey = null;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => context.State(nullKey!, 0));
    }

    [Fact]
    public void State_ThrowsWhenKeyIsEmpty()
    {
        // Arrange
        var context = CreateViewContext();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => context.State("", 0));
    }

    [Fact]
    public void State_ThrowsWhenKeyIsWhitespace()
    {
        // Arrange
        var context = CreateViewContext();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => context.State("   ", 0));
    }

    [Fact]
    public void StateGlobal_ThrowsWhenKeyIsNull()
    {
        // Arrange
        var context = CreateViewContext();
        string? nullKey = null;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => context.StateGlobal(nullKey!, 0));
    }

    [Fact]
    public void StateGlobal_ThrowsWhenKeyIsEmpty()
    {
        // Arrange
        var context = CreateViewContext();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => context.StateGlobal("", 0));
    }

    [Fact]
    public void StateGlobal_ThrowsWhenKeyIsWhitespace()
    {
        // Arrange
        var context = CreateViewContext();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => context.StateGlobal("   ", 0));
    }

    [Fact]
    public void State_WorksWithValidKey()
    {
        // Arrange
        var context = CreateViewContext();

        // Act
        var state = context.State("validKey", 42);

        // Assert
        Assert.NotNull(state);
        Assert.Equal(42, state.Value);
    }

    [Fact]
    public void StateGlobal_WorksWithValidKey()
    {
        // Arrange
        var context = CreateViewContext();

        // Act
        var state = context.StateGlobal("validKey", 42);

        // Assert
        Assert.NotNull(state);
        Assert.Equal(42, state.Value);
    }

    private ViewContext CreateViewContext()
    {
        var services = new ServiceCollection();
        services.AddRndr();
        var serviceProvider = services.BuildServiceProvider();

        var navigationStack = new Stack<string>();
        navigationStack.Push("/");
        var navigationState = new TestNavigationState(navigationStack);
        var navigationContext = new NavigationContext(navigationState);
        var stateStore = new InMemoryStateStore();

        return new ViewContext(
            serviceProvider,
            navigationContext,
            NullLogger.Instance,
            "/",
            stateStore,
            () => { });
    }

    private sealed class TestNavigationState : INavigationState
    {
        private readonly Stack<string> _stack;

        public TestNavigationState(Stack<string> stack) => _stack = stack;
        public string CurrentRoute => _stack.Count > 0 ? _stack.Peek() : "/";
        public IReadOnlyList<string> Stack => _stack.ToArray().Reverse().ToList();
    }
}
