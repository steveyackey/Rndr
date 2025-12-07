using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rndr.Extensions;
using Rndr.Layout;
using Xunit;

namespace Rndr.Tests;

public class GlobalContextTests
{
    [Fact]
    public void StateGlobal_CreatesAndReturnsState()
    {
        // Arrange
        var builder = TuiApplication.CreateBuilder(Array.Empty<string>());
        var app = builder.Build();
        
        var navigationState = new TestNavigationState();
        var navigationContext = new Navigation.NavigationContext(navigationState);
        var globalContext = new GlobalContext(navigationContext, app);

        // Act
        var state = globalContext.StateGlobal("test", 42);

        // Assert
        Assert.NotNull(state);
        Assert.Equal(42, state.Value);
    }

    [Fact]
    public void StateGlobal_ReturnsSameStateForSameKey()
    {
        // Arrange
        var builder = TuiApplication.CreateBuilder(Array.Empty<string>());
        var app = builder.Build();
        
        var navigationState = new TestNavigationState();
        var navigationContext = new Navigation.NavigationContext(navigationState);
        var globalContext = new GlobalContext(navigationContext, app);

        // Act
        var state1 = globalContext.StateGlobal("test", 42);
        state1.Value = 100;
        var state2 = globalContext.StateGlobal("test", 42);

        // Assert
        Assert.Equal(100, state2.Value);
    }

    [Fact]
    public void StateGlobal_ThrowsWhenKeyIsNull()
    {
        // Arrange
        var builder = TuiApplication.CreateBuilder(Array.Empty<string>());
        var app = builder.Build();
        
        var navigationState = new TestNavigationState();
        var navigationContext = new Navigation.NavigationContext(navigationState);
        var globalContext = new GlobalContext(navigationContext, app);
        string? nullKey = null;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => globalContext.StateGlobal(nullKey!, 0));
    }

    [Fact]
    public void StateGlobal_ThrowsWhenKeyIsEmpty()
    {
        // Arrange
        var builder = TuiApplication.CreateBuilder(Array.Empty<string>());
        var app = builder.Build();
        
        var navigationState = new TestNavigationState();
        var navigationContext = new Navigation.NavigationContext(navigationState);
        var globalContext = new GlobalContext(navigationContext, app);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => globalContext.StateGlobal("", 0));
    }

    [Fact]
    public void StateGlobal_ThrowsWhenKeyIsWhitespace()
    {
        // Arrange
        var builder = TuiApplication.CreateBuilder(Array.Empty<string>());
        var app = builder.Build();
        
        var navigationState = new TestNavigationState();
        var navigationContext = new Navigation.NavigationContext(navigationState);
        var globalContext = new GlobalContext(navigationContext, app);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => globalContext.StateGlobal("   ", 0));
    }

    private sealed class TestNavigationState : Navigation.INavigationState
    {
        public string CurrentRoute => "/";
        public IReadOnlyList<string> Stack => new[] { "/" };
    }
}
