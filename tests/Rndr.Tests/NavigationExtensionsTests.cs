using Rndr.Extensions;
using Rndr.Navigation;
using Rndr.Testing;
using Xunit;

namespace Rndr.Tests;

public class NavigationExtensionsTests
{
    [Fact]
    public void NavigateHome_NavigatesToRoot()
    {
        // Arrange
        var navigationStack = new Stack<string>();
        navigationStack.Push("/settings");
        var navigationState = new TestNavigationState(navigationStack);
        string? navigatedRoute = null;
        var navigation = new NavigationContext(
            navigationState,
            onNavigate: route => navigatedRoute = route);

        // Act
        navigation.NavigateHome();

        // Assert
        Assert.Equal("/", navigatedRoute);
    }

    [Fact]
    public void CanGoBack_ReturnsTrueWhenStackHasMultipleRoutes()
    {
        // Arrange
        var navigationStack = new Stack<string>();
        navigationStack.Push("/");
        navigationStack.Push("/settings");
        var navigationState = new TestNavigationState(navigationStack);
        var navigation = new NavigationContext(navigationState);

        // Act
        var canGoBack = navigation.CanGoBack();

        // Assert
        Assert.True(canGoBack);
    }

    [Fact]
    public void CanGoBack_ReturnsFalseWhenStackHasOneRoute()
    {
        // Arrange
        var navigationStack = new Stack<string>();
        navigationStack.Push("/");
        var navigationState = new TestNavigationState(navigationStack);
        var navigation = new NavigationContext(navigationState);

        // Act
        var canGoBack = navigation.CanGoBack();

        // Assert
        Assert.False(canGoBack);
    }

    [Fact]
    public void BackOrHome_CallsBackWhenCanGoBack()
    {
        // Arrange
        var navigationStack = new Stack<string>();
        navigationStack.Push("/");
        navigationStack.Push("/settings");
        var navigationState = new TestNavigationState(navigationStack);
        bool backCalled = false;
        var navigation = new NavigationContext(
            navigationState,
            onBack: () => { backCalled = true; return true; });

        // Act
        navigation.BackOrHome();

        // Assert
        Assert.True(backCalled);
    }

    [Fact]
    public void BackOrHome_NavigatesHomeWhenCannotGoBack()
    {
        // Arrange
        var navigationStack = new Stack<string>();
        navigationStack.Push("/settings");
        var navigationState = new TestNavigationState(navigationStack);
        string? navigatedRoute = null;
        var navigation = new NavigationContext(
            navigationState,
            onNavigate: route => navigatedRoute = route,
            onBack: () => false);

        // Act
        navigation.BackOrHome();

        // Assert
        Assert.Equal("/", navigatedRoute);
    }

    private sealed class TestNavigationState : INavigationState
    {
        private readonly Stack<string> _stack;

        public TestNavigationState(Stack<string> stack) => _stack = stack;
        public string CurrentRoute => _stack.Count > 0 ? _stack.Peek() : "/";
        public IReadOnlyList<string> Stack => _stack.ToArray().Reverse().ToList();
    }
}
