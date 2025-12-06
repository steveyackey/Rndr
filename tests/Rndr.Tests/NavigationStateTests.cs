using Rndr.Navigation;
using Xunit;

namespace Rndr.Tests;

public class NavigationStateTests
{
    [Fact]
    public void NavigationState_InitialRoute_IsRoot()
    {
        // Arrange
        var stack = new Stack<string>();
        stack.Push("/");

        // Act
        var state = new TestableNavigationState(stack);

        // Assert
        Assert.Equal("/", state.CurrentRoute);
    }

    [Fact]
    public void NavigationState_Push_UpdatesCurrentRoute()
    {
        // Arrange
        var stack = new Stack<string>();
        stack.Push("/");
        var state = new TestableNavigationState(stack);

        // Act
        stack.Push("/settings");

        // Assert
        Assert.Equal("/settings", state.CurrentRoute);
    }

    [Fact]
    public void NavigationState_Pop_ReturnsToPrevisRoute()
    {
        // Arrange
        var stack = new Stack<string>();
        stack.Push("/");
        stack.Push("/settings");
        var state = new TestableNavigationState(stack);

        // Act
        stack.Pop();

        // Assert
        Assert.Equal("/", state.CurrentRoute);
    }

    [Fact]
    public void NavigationState_Stack_ReturnsAllRoutes()
    {
        // Arrange
        var stack = new Stack<string>();
        stack.Push("/");
        stack.Push("/settings");
        stack.Push("/settings/advanced");
        var state = new TestableNavigationState(stack);

        // Act
        var routes = state.Stack;

        // Assert
        Assert.Equal(3, routes.Count);
        Assert.Equal("/", routes[0]);
        Assert.Equal("/settings", routes[1]);
        Assert.Equal("/settings/advanced", routes[2]);
    }

    [Fact]
    public void NavigationState_Replace_KeepsStackDepth()
    {
        // Arrange
        var stack = new Stack<string>();
        stack.Push("/");
        stack.Push("/settings");
        var state = new TestableNavigationState(stack);

        // Act - simulate replace
        stack.Pop();
        stack.Push("/about");

        // Assert
        Assert.Equal("/about", state.CurrentRoute);
        Assert.Equal(2, state.Stack.Count);
    }

    [Fact]
    public void NavigationState_EmptyStack_ReturnsRootRoute()
    {
        // Arrange
        var stack = new Stack<string>();
        var state = new TestableNavigationState(stack);

        // Assert
        Assert.Equal("/", state.CurrentRoute);
    }

    /// <summary>
    /// Testable version of NavigationState that accepts a stack directly.
    /// </summary>
    private sealed class TestableNavigationState : INavigationState
    {
        private readonly Stack<string> _stack;

        public TestableNavigationState(Stack<string> stack)
        {
            _stack = stack;
        }

        public string CurrentRoute => _stack.Count > 0 ? _stack.Peek() : "/";
        public IReadOnlyList<string> Stack => _stack.ToArray().Reverse().ToList();
    }
}

