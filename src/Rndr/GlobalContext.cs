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
}

