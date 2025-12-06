namespace Rndr.Navigation;

/// <summary>
/// Observable navigation state for testing and diagnostics.
/// </summary>
public interface INavigationState
{
    /// <summary>
    /// Gets the current route path.
    /// </summary>
    string CurrentRoute { get; }

    /// <summary>
    /// Gets the full navigation stack.
    /// </summary>
    IReadOnlyList<string> Stack { get; }
}

