namespace Rndr.Navigation;

/// <summary>
/// Provides navigation operations for views.
/// </summary>
public sealed class NavigationContext
{
    private readonly INavigationState _state;
    private readonly Action<string>? _onNavigate;
    private readonly Func<bool>? _onBack;
    private readonly Action<string>? _onReplace;

    internal NavigationContext(
        INavigationState state,
        Action<string>? onNavigate = null,
        Func<bool>? onBack = null,
        Action<string>? onReplace = null)
    {
        _state = state;
        _onNavigate = onNavigate;
        _onBack = onBack;
        _onReplace = onReplace;
    }

    /// <summary>
    /// Gets the current route path.
    /// </summary>
    public string CurrentRoute => _state.CurrentRoute;

    /// <summary>
    /// Navigates to a new route, pushing it onto the navigation stack.
    /// </summary>
    /// <param name="route">The route to navigate to.</param>
    /// <returns>True if navigation succeeded.</returns>
    public bool Navigate(string route)
    {
        _onNavigate?.Invoke(route);
        return true;
    }

    /// <summary>
    /// Navigates back to the previous route.
    /// </summary>
    /// <returns>True if there was a route to go back to.</returns>
    public bool Back()
    {
        return _onBack?.Invoke() ?? false;
    }

    /// <summary>
    /// Replaces the current route without changing the stack.
    /// </summary>
    /// <param name="route">The route to replace with.</param>
    /// <returns>True if replacement succeeded.</returns>
    public bool Replace(string route)
    {
        _onReplace?.Invoke(route);
        return true;
    }
}

