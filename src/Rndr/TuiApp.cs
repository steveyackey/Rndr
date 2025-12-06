using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rndr.Diagnostics;
using Rndr.Input;
using Rndr.Layout;
using Rndr.Navigation;
using Rndr.Rendering;

namespace Rndr;

/// <summary>
/// Represents a running TUI application instance.
/// </summary>
public sealed class TuiApp : IAsyncDisposable
{
    private readonly IHost _host;
    private readonly Dictionary<string, ViewRegistration> _routes = [];
    private readonly List<Func<KeyEvent, GlobalContext, bool>> _globalKeyHandlers = [];
    private readonly Stack<string> _navigationStack = new();
    private readonly NavigationState _navigationState;

    private CancellationTokenSource? _cts;

    internal TuiApp(IHost host)
    {
        _host = host;
        _navigationState = new NavigationState(_navigationStack);
        _navigationStack.Push("/");
    }

    /// <summary>
    /// Gets the service provider for resolving dependencies.
    /// </summary>
    public IServiceProvider Services => _host.Services;

    /// <summary>
    /// Gets the current route path.
    /// </summary>
    public string CurrentRoute => _navigationStack.Peek();

    /// <summary>
    /// Gets a value indicating whether the application is quitting.
    /// </summary>
    public bool IsQuitting { get; private set; }

    /// <summary>
    /// Maps a route to a C# view builder.
    /// </summary>
    /// <param name="route">The route path (e.g., "/", "/settings").</param>
    /// <param name="configure">Action to configure the view definition.</param>
    /// <returns>This app instance for chaining.</returns>
    public TuiApp MapView(string route, Action<ViewDefinition> configure)
    {
        var definition = new ViewDefinition();
        configure(definition);

        _routes[route] = new ViewRegistration
        {
            Route = route,
            ViewBuilder = configure,
            LayoutBuilder = definition.ViewLayoutBuilder,
            Title = definition.ViewTitle,
            KeyHandler = definition.KeyHandler
        };

        return this;
    }

    /// <summary>
    /// Maps a route to a .tui component type.
    /// </summary>
    /// <param name="route">The route path.</param>
    /// <param name="viewComponentType">The component type to instantiate.</param>
    /// <returns>This app instance for chaining.</returns>
    public TuiApp MapView(string route, Type viewComponentType)
    {
        _routes[route] = new ViewRegistration
        {
            Route = route,
            ComponentType = viewComponentType
        };

        return this;
    }

    /// <summary>
    /// Registers a global key handler that runs before view-specific handlers.
    /// </summary>
    /// <param name="handler">Handler that returns true if the key was consumed.</param>
    /// <returns>This app instance for chaining.</returns>
    public TuiApp OnGlobalKey(Func<KeyEvent, GlobalContext, bool> handler)
    {
        _globalKeyHandlers.Add(handler);
        return this;
    }

    /// <summary>
    /// Runs the application event loop.
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        var eventLoop = Services.GetRequiredService<IEventLoop>();

        try
        {
            await eventLoop.RunAsync(this, _cts.Token).ConfigureAwait(false);
        }
        finally
        {
            // Restore console state
            var console = Services.GetRequiredService<IConsoleAdapter>();
            console.ShowCursor();
            console.Clear();
            console.Flush();
        }
    }

    /// <summary>
    /// Signals the application to quit.
    /// </summary>
    public void Quit()
    {
        IsQuitting = true;
        _cts?.Cancel();
    }

    /// <summary>
    /// Navigates to a new route.
    /// </summary>
    /// <param name="route">The route to navigate to.</param>
    internal void Navigate(string route)
    {
        if (!_routes.ContainsKey(route))
        {
            return;
        }

        var fromRoute = CurrentRoute;
        _navigationStack.Push(route);

        using var activity = RndrDiagnostics.StartNavigationActivity(fromRoute, route);
        RndrDiagnostics.RecordNavigation(fromRoute, route);
    }

    /// <summary>
    /// Navigates back to the previous route.
    /// </summary>
    /// <returns>True if there was a route to go back to.</returns>
    internal bool Back()
    {
        if (_navigationStack.Count <= 1)
        {
            return false;
        }

        var fromRoute = _navigationStack.Pop();
        var toRoute = CurrentRoute;

        using var activity = RndrDiagnostics.StartNavigationActivity(fromRoute, toRoute);
        RndrDiagnostics.RecordNavigation(fromRoute, toRoute);

        return true;
    }

    /// <summary>
    /// Replaces the current route.
    /// </summary>
    /// <param name="route">The route to replace with.</param>
    internal void Replace(string route)
    {
        if (!_routes.ContainsKey(route))
        {
            return;
        }

        var fromRoute = CurrentRoute;
        _navigationStack.Pop();
        _navigationStack.Push(route);

        using var activity = RndrDiagnostics.StartNavigationActivity(fromRoute, route);
        RndrDiagnostics.RecordNavigation(fromRoute, route);
    }

    /// <summary>
    /// Gets the view registration for a route.
    /// </summary>
    internal ViewRegistration? GetViewRegistration(string route)
    {
        return _routes.TryGetValue(route, out var reg) ? reg : null;
    }

    /// <summary>
    /// Gets the navigation state.
    /// </summary>
    internal INavigationState GetNavigationState() => _navigationState;

    /// <summary>
    /// Handles a global key event.
    /// </summary>
    /// <returns>True if the key was consumed.</returns>
    internal bool HandleGlobalKey(KeyEvent key)
    {
        var globalContext = new GlobalContext(
            new NavigationContext(
                _navigationState,
                Navigate,
                Back,
                Replace
            ),
            this
        );

        foreach (var handler in _globalKeyHandlers)
        {
            if (handler(key, globalContext))
            {
                return true;
            }
        }

        return false;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _host.Dispose();
        await Task.CompletedTask;
    }
}

/// <summary>
/// Internal representation of a registered view.
/// </summary>
internal sealed class ViewRegistration
{
    public required string Route { get; init; }
    public Action<ViewDefinition>? ViewBuilder { get; init; }
    public Action<ViewContext, LayoutBuilder>? LayoutBuilder { get; init; }
    public Type? ComponentType { get; init; }
    public string? Title { get; init; }
    public Func<KeyEvent, ViewContext, bool>? KeyHandler { get; init; }
}

