using Microsoft.Extensions.DependencyInjection;
using Rndr.Navigation;

namespace Rndr;

/// <summary>
/// Context provided to global key handlers.
/// </summary>
public sealed class GlobalContext
{
    internal GlobalContext(NavigationContext navigation, TuiApp application)
    {
        Navigation = navigation;
        Application = application;
    }

    /// <summary>
    /// Gets the navigation context for navigating between views.
    /// </summary>
    public NavigationContext Navigation { get; }

    /// <summary>
    /// Gets the application instance for controlling the app lifecycle.
    /// </summary>
    public TuiApp Application { get; }

    /// <summary>
    /// Gets or creates global state that persists across navigation.
    /// Provides a simpler API than accessing the state store through services.
    /// </summary>
    /// <typeparam name="T">The type of the state value.</typeparam>
    /// <param name="key">The state key. Must not be null or empty.</param>
    /// <param name="initialValue">The initial value if the state doesn't exist.</param>
    /// <returns>A signal containing the state value.</returns>
    /// <exception cref="ArgumentException">Thrown when key is null or empty.</exception>
    public Signal<T> StateGlobal<T>(string key, T initialValue)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("State key cannot be null or empty.", nameof(key));
        }

        var stateStore = Application.Services.GetRequiredService<IStateStore>();
        return stateStore.GetOrCreate("global", key, () => initialValue);
    }
}

