using Microsoft.Extensions.Logging;
using Rndr.Navigation;

namespace Rndr;

/// <summary>
/// Context provided to views during rendering, containing services and state management.
/// </summary>
public sealed class ViewContext
{
    private readonly IStateStore _stateStore;
    private readonly Action _onStateChanged;

    internal ViewContext(
        IServiceProvider services,
        NavigationContext navigation,
        ILogger logger,
        string route,
        IStateStore stateStore,
        Action onStateChanged)
    {
        Services = services;
        Navigation = navigation;
        Logger = logger;
        Route = route;
        _stateStore = stateStore;
        _onStateChanged = onStateChanged;
    }

    /// <summary>
    /// Gets the service provider for dependency injection.
    /// </summary>
    public IServiceProvider Services { get; }

    /// <summary>
    /// Gets the navigation context for navigating between views.
    /// </summary>
    public NavigationContext Navigation { get; }

    /// <summary>
    /// Gets the logger scoped to this view.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Gets the current route path.
    /// </summary>
    public string Route { get; }

    /// <summary>
    /// Gets or creates route-scoped state.
    /// </summary>
    /// <typeparam name="T">The type of the state value.</typeparam>
    /// <param name="key">The state key.</param>
    /// <param name="initialValue">The initial value if the state doesn't exist.</param>
    /// <returns>A signal containing the state value.</returns>
    public Signal<T> State<T>(string key, T initialValue)
    {
        return _stateStore.GetOrCreate(Route, key, () => initialValue, _onStateChanged);
    }

    /// <summary>
    /// Gets or creates global state that persists across navigation.
    /// </summary>
    /// <typeparam name="T">The type of the state value.</typeparam>
    /// <param name="key">The state key.</param>
    /// <param name="initialValue">The initial value if the state doesn't exist.</param>
    /// <returns>A signal containing the state value.</returns>
    public Signal<T> StateGlobal<T>(string key, T initialValue)
    {
        return _stateStore.GetOrCreate("global", key, () => initialValue, _onStateChanged);
    }
}

