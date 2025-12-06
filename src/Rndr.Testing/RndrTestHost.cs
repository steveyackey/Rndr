using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Rndr.Layout;
using Rndr.Navigation;

namespace Rndr.Testing;

/// <summary>
/// Test host for building and testing Rndr components in isolation.
/// </summary>
public static class RndrTestHost
{
    /// <summary>
    /// Builds a component and returns its layout nodes and context.
    /// </summary>
    /// <typeparam name="T">The component type to build.</typeparam>
    /// <param name="services">Optional service provider for dependency injection.</param>
    /// <param name="stateStore">Optional state store (creates InMemoryStateStore if null).</param>
    /// <returns>A tuple of the built nodes and the view context.</returns>
    public static (IReadOnlyList<Node> Nodes, ViewContext Context) BuildComponent<T>(
        IServiceProvider? services = null,
        IStateStore? stateStore = null)
        where T : TuiComponentBase, new()
    {
        services ??= new ServiceCollection().BuildServiceProvider();
        stateStore ??= new InMemoryStateStore();

        var navigationStack = new Stack<string>();
        navigationStack.Push("/");
        var navigationState = new TestNavigationState(navigationStack);
        var navigationContext = new NavigationContext(navigationState);

        var context = new ViewContext(
            services,
            navigationContext,
            NullLogger.Instance,
            "/",
            stateStore,
            () => { } // No-op for tests
        );

        var component = new T();
        component.AttachContext(context);

        var layout = new LayoutBuilder();
        component.Build(layout);

        return (layout.Build(), context);
    }

    /// <summary>
    /// Builds a view definition and returns its layout nodes.
    /// </summary>
    /// <param name="configure">The view configuration action.</param>
    /// <param name="services">Optional service provider.</param>
    /// <param name="stateStore">Optional state store.</param>
    /// <returns>A tuple of the built nodes and the view context.</returns>
    public static (IReadOnlyList<Node> Nodes, ViewContext Context) BuildView(
        Action<ViewDefinition> configure,
        IServiceProvider? services = null,
        IStateStore? stateStore = null)
    {
        services ??= new ServiceCollection().BuildServiceProvider();
        stateStore ??= new InMemoryStateStore();

        var navigationStack = new Stack<string>();
        navigationStack.Push("/");
        var navigationState = new TestNavigationState(navigationStack);
        var navigationContext = new NavigationContext(navigationState);

        var context = new ViewContext(
            services,
            navigationContext,
            NullLogger.Instance,
            "/",
            stateStore,
            () => { }
        );

        var definition = new ViewDefinition();
        configure(definition);

        var layout = new LayoutBuilder();
        definition.ViewLayoutBuilder?.Invoke(context, layout);

        return (layout.Build(), context);
    }

    private sealed class TestNavigationState : INavigationState
    {
        private readonly Stack<string> _stack;

        public TestNavigationState(Stack<string> stack) => _stack = stack;
        public string CurrentRoute => _stack.Count > 0 ? _stack.Peek() : "/";
        public IReadOnlyList<string> Stack => _stack.ToArray().Reverse().ToList();
    }
}

